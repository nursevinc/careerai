using System;
using System.Collections.Generic;
using System.Text;

using CareerAI.Application.DTOs.Account;
using CareerAI.Application.DTOs.Auth;
using CareerAI.Application.Validators;
using Xunit;

namespace CareerAI.Tests;

public class AccountValidatorsTests
{
    private readonly ChangePasswordRequestValidator _changePassword = new();
    private readonly RegisterRequestValidator _register = new();

    [Fact]
    public void ChangePassword_ValidRequest_Passes()
    {
        var result = _changePassword.Validate(new ChangePasswordRequest("Eski1234", "Yeni5678"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void ChangePassword_NewPasswordSameAsCurrent_Fails()
    {
        var result = _changePassword.Validate(new ChangePasswordRequest("Sifre1234", "Sifre1234"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "NewPassword");
    }

    [Theory]
    [InlineData("short1")]        // 8 karakterden kısa
    [InlineData("onlyletters")]   // rakam yok
    [InlineData("12345678")]      // harf yok
    public void ChangePassword_WeakNewPassword_Fails(string weak)
    {
        var result = _changePassword.Validate(new ChangePasswordRequest("Eski1234", weak));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void ChangePassword_NewPasswordLongerThanBCryptLimit_Fails()
    {
        // BCrypt 72 byte'tan sonrasını yok sayar: üst sınır 64
        var result = _changePassword.Validate(new ChangePasswordRequest("Eski1234", "A1" + new string('x', 70)));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Register_UsesTheSamePasswordPolicy()
    {
        Assert.False(_register.Validate(new RegisterRequest("Nur", "nur@test.com", "short1")).IsValid);
        Assert.True(_register.Validate(new RegisterRequest("Nur", "nur@test.com", "Test1234")).IsValid);
    }
}