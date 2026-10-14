using System;
using System.Collections.Generic;
using System.Text;

using CareerAI.Application.DTOs.Analysis;
using FluentValidation;

namespace CareerAI.Application.Validators;

public class AnalyzeRequestValidator : AbstractValidator<AnalyzeRequest>
{
    public AnalyzeRequestValidator()
    {
        RuleFor(x => x.ResumeId).NotEmpty();
    }
}