using System;
using System.Collections.Generic;
using System.Text;

using CareerAI.Application.DTOs.Ai;
using CareerAI.Application.Services;
using Xunit;

namespace CareerAI.Tests;

public class SkillSanitizerTests
{
    [Fact]
    public void CleanSkills_TrimsAndRemovesDuplicatesAndBlankItems()
    {
        var result = SkillSanitizer.CleanSkills(["  C#  ", "c#", "", "   ", "Docker", null]);

        Assert.Equal(new[] { "C#", "Docker" }, result);
    }

    [Fact]
    public void CleanSkills_DropsTooLongItems()
    {
        var result = SkillSanitizer.CleanSkills([new string('a', SkillSanitizer.MaxSkillLength + 1), "Git"]);

        Assert.Equal(new[] { "Git" }, result);
    }

    [Fact]
    public void KeepGrounded_DropsSkillsWhoseEvidenceIsNotInTheSource()
    {
        var skills = new List<ExtractedSkill>
        {
            new("Prompt Engineering", "Prompt Mühendisliği"),
            new("Kubernetes", "Kubernetes")
        };

        var kept = SkillSanitizer.KeepGrounded(skills, "Deneyim: Prompt Mühendisliği, Docker", out var dropped);

        Assert.Single(kept);
        Assert.Equal("Prompt Engineering", kept[0].Name);
        Assert.Equal(1, dropped);
    }

    [Fact]
    public void NormalizeExperienceLevel_FallsBackToUnspecified()
    {
        Assert.Equal("Senior", SkillSanitizer.NormalizeExperienceLevel("senior"));
        Assert.Equal("Unspecified", SkillSanitizer.NormalizeExperienceLevel("wizard"));
        Assert.Equal("Unspecified", SkillSanitizer.NormalizeExperienceLevel(null));
    }
}