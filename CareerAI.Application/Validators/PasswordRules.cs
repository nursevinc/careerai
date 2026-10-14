using System;
using System.Collections.Generic;
using System.Text;

using FluentValidation;

namespace CareerAI.Application.Validators;

public static class PasswordRules
{
    // Şifre politikası tek yerde: kayıt ve şifre değiştirme aynı kuralları kullanır.
    // Üst sınır: BCrypt 72 byte'tan sonrasını yok sayar.
    public static IRuleBuilderOptions<T, string> MustBeValidPassword<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
            .MinimumLength(8)
            .MaximumLength(64)
            .Matches("[A-Za-z]").WithMessage("Password must contain at least one letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.");
}