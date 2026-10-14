using System;
using System.Collections.Generic;
using System.Text;

using CareerAI.Application.DTOs.Analysis;

namespace CareerAI.Application.Interfaces;

public interface IMatchingService
{
    MatchResult CalculateMatch(
        IEnumerable<string> candidateSkills,
        IEnumerable<string> requiredSkills,
        IEnumerable<string> preferredSkills);
}