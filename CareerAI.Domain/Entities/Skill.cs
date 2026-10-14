using System;
using System.Collections.Generic;
using System.Text;

using CareerAI.Domain.Common;

namespace CareerAI.Domain.Entities;

public class Skill : BaseEntity
{
    public string Name { get; set; } = string.Empty;
}