using CareerAI.Domain.Common;
using CareerAI.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace CareerAI.Domain.Entities;

public class JobApplicationSkill : BaseEntity
{
    public Guid JobApplicationId { get; set; }
    public JobApplication JobApplication { get; set; } = null!;

    public Guid SkillId { get; set; }
    public Skill Skill { get; set; } = null!;

    // İlanda bu skill zorunlu mu, tercih mi?
    public SkillRequirementType RequirementType { get; set; }

    // CV'de bu skill bulundu mu? (matching algoritması sonucu)
    public bool IsMatched { get; set; }
}