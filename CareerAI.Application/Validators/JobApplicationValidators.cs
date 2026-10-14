using System;
using System.Collections.Generic;
using System.Text;

using CareerAI.Application.Common;
using CareerAI.Application.DTOs.JobApplications;
using FluentValidation;

namespace CareerAI.Application.Validators;

public class CreateJobApplicationRequestValidator : AbstractValidator<CreateJobApplicationRequest>
{
    public CreateJobApplicationRequestValidator()
    {
        RuleFor(x => x.JobTitle).NotEmpty().MaximumLength(JobApplicationLimits.MaxNameLength);

        RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(JobApplicationLimits.MaxNameLength);

        RuleFor(x => x.JobDescription)
            .Cascade(CascadeMode.Stop) // NotEmpty başarısızsa sonraki kurallar çalışmasın (null güvenliği)
            .NotEmpty()
            .MaximumLength(JobApplicationLimits.MaxDescriptionLength)
            .Must(description => description.Trim().Length >= JobApplicationLimits.MinDescriptionLength)
            .WithMessage($"Job description must be at least {JobApplicationLimits.MinDescriptionLength} characters.");
    }
}