using System;
using System.Collections.Generic;
using System.Text;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CareerAI.Application.DTOs.Account;
using CareerAI.Application.DTOs.Auth;
using CareerAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using static CareerAI.Tests.Integration.ApiTestHelpers;

namespace CareerAI.Tests.Integration;

[Collection(ApiCollection.Name)]
public class AuthApiTests(CareerApiFactory factory)
{
    [Fact]
    public async Task Register_Login_Me_FullFlow_Works_AndPasswordIsHashed()
    {
        var email = $"flow-{Guid.NewGuid():N}@test.com";
        var client = factory.CreateClient();

        var register = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Flow User", email, DefaultPassword));
        Assert.Equal(HttpStatusCode.Created, register.StatusCode);

        // E-posta büyük harfle de çalışmalı (normalizasyon)
        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email.ToUpperInvariant(), DefaultPassword));
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        var auth = await login.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        client.UseToken(auth.AccessToken);

        var me = await client.GetFromJsonAsync<UserDto>("/api/auth/me");
        Assert.NotNull(me);
        Assert.Equal(email, me.Email);

        // Şifre düz metin olarak saklanmıyor: BCrypt hash'i
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users.AsNoTracking().SingleAsync(u => u.Email == email);

        Assert.NotEqual(DefaultPassword, user.PasswordHash);
        Assert.StartsWith("$2", user.PasswordHash);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        var user = await factory.RegisterUserAsync();

        var response = await factory.CreateClient().PostAsJsonAsync(
            "/api/auth/register", new RegisterRequest("Another", user.Email, DefaultPassword));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Register_WeakPassword_ReturnsValidationErrors()
    {
        var response = await factory.CreateClient().PostAsJsonAsync(
            "/api/auth/register", new RegisterRequest("Weak", $"weak-{Guid.NewGuid():N}@test.com", "123"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.GetProperty("errors").TryGetProperty("password", out _));
    }

    [Fact]
    public async Task Login_WrongPasswordAndUnknownEmail_ReturnSameUnauthorizedError()
    {
        var user = await factory.RegisterUserAsync();
        var client = factory.CreateClient();

        var wrongPassword = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(user.Email, "WrongPass1"));
        var unknownEmail = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("nobody@test.com", "WrongPass1"));

        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unknownEmail.StatusCode);

        // Kullanıcı sayımı (user enumeration) engeli: iki durumda da AYNI mesaj
        var first = await wrongPassword.ReadProblemAsync();
        var second = await unknownEmail.ReadProblemAsync();
        Assert.Equal(first.Detail, second.Detail);
    }

    [Fact]
    public async Task ChangePassword_RevokesOldToken_AndReturnsNewWorkingToken()
    {
        var user = await factory.RegisterUserAsync();
        const string newPassword = "NewPass5678";

        var response = await user.Client.PostAsJsonAsync(
            "/api/account/change-password", new ChangePasswordRequest(user.Password, newPassword));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var renewed = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(renewed);

        // Eski token (diğer cihazlardaki oturum) artık geçersiz
        await user.Client.GetAsync("/api/auth/me").AssertStatusAsync(HttpStatusCode.Unauthorized);

        // Yeni token çalışıyor
        var newClient = factory.CreateClient();
        newClient.UseToken(renewed.AccessToken);
        await newClient.GetAsync("/api/auth/me").AssertStatusAsync(HttpStatusCode.OK);

        // Yeni şifreyle giriş olur, eskisiyle olmaz
        var anonymous = factory.CreateClient();
        await anonymous.PostAsJsonAsync("/api/auth/login", new LoginRequest(user.Email, newPassword))
            .AssertStatusAsync(HttpStatusCode.OK);
        await anonymous.PostAsJsonAsync("/api/auth/login", new LoginRequest(user.Email, user.Password))
            .AssertStatusAsync(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangePassword_WrongCurrentPassword_ReturnsBadRequestNotUnauthorized()
    {
        var user = await factory.RegisterUserAsync();

        var response = await user.Client.PostAsJsonAsync(
            "/api/account/change-password", new ChangePasswordRequest("WrongPass1", "NewPass5678"));

        // 401 olsaydı frontend kullanıcıyı sistemden atardı: bu bir form hatası, oturum hatası değil
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("invalid_password", (await response.ReadProblemAsync()).Code);
    }

    [Fact]
    public async Task DeleteAccount_RemovesAllUserData_AndInvalidatesToken()
    {
        var user = await factory.RegisterUserAsync();
        var resume = await user.Client.UploadResumeAsync();
        await user.Client.CreateJobAsync();

        // Yanlış şifreyle silinmez
        var wrong = new HttpRequestMessage(HttpMethod.Delete, "/api/account")
        {
            Content = JsonContent.Create(new DeleteAccountRequest("WrongPass1"))
        };
        Assert.Equal(HttpStatusCode.BadRequest, (await user.Client.SendAsync(wrong)).StatusCode);

        var delete = new HttpRequestMessage(HttpMethod.Delete, "/api/account")
        {
            Content = JsonContent.Create(new DeleteAccountRequest(user.Password))
        };
        Assert.Equal(HttpStatusCode.NoContent, (await user.Client.SendAsync(delete)).StatusCode);

        // Token artık geçersiz, giriş yapılamıyor
        await user.Client.GetAsync("/api/auth/me").AssertStatusAsync(HttpStatusCode.Unauthorized);
        await factory.CreateClient().PostAsJsonAsync("/api/auth/login", new LoginRequest(user.Email, user.Password))
            .AssertStatusAsync(HttpStatusCode.Unauthorized);

        // Veritabanında hiçbir iz kalmadı
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.False(await db.Users.AnyAsync(u => u.Id == user.Id));
        Assert.False(await db.Resumes.AnyAsync(r => r.UserId == user.Id));
        Assert.False(await db.JobApplications.AnyAsync(a => a.UserId == user.Id));

        // CV dosyası da diskten silindi
        Assert.DoesNotContain(
            Directory.GetFiles(factory.UploadsPath),
            path => path.Contains(resume.Id.ToString("N"), StringComparison.OrdinalIgnoreCase));
    }
}