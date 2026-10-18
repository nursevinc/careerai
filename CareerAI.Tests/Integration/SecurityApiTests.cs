using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using Xunit;

namespace CareerAI.Tests.Integration;

[Collection(ApiCollection.Name)]
public class SecurityApiTests(CareerApiFactory factory)
{
    [Theory]
    [InlineData("/api/auth/me")]
    [InlineData("/api/resumes")]
    [InlineData("/api/job-applications")]
    [InlineData("/api/dashboard")]
    public async Task ProtectedEndpoints_RequireAuthentication(string url)
    {
        var response = await factory.CreateClient().GetAsync(url);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Upload_WithoutToken_IsRejected()
    {
        var client = factory.CreateClient();

        var response = await client.UploadAsync(TestPdf.Create("hello"), "cv.pdf");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TamperedToken_IsRejected()
    {
        var user = await factory.RegisterUserAsync();
        var client = factory.CreateClient();
        client.UseToken(user.Auth.AccessToken[..^3] + "abc"); // imza bozuldu

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Responses_CarrySecurityHeaders()
    {
        var response = await factory.CreateClient().GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").Single());
        Assert.Equal("no-referrer", response.Headers.GetValues("Referrer-Policy").Single());
        Assert.True(response.Headers.CacheControl?.NoStore);
        Assert.Contains("default-src 'none'", response.Headers.GetValues("Content-Security-Policy").Single());
    }
}