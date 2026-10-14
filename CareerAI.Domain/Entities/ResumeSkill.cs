using System;
using System.Collections.Generic;
using System.Text;

using CareerAI.Domain.Common;

namespace CareerAI.Domain.Entities;

public class ResumeSkill : BaseEntity
{
    public Guid ResumeId { get; set; }
    public Resume Resume { get; set; } = null!;

    public Guid SkillId { get; set; }
    public Skill Skill { get; set; } = null!;
}