using System;
using System.Collections.Generic;
using System.Text;

namespace CareerAI.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; set; }
}