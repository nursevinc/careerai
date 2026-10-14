using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CareerAI.Infrastructure.Data;

public static class DbUpdateExceptionExtensions
{
    // SQL Server: 2601 (unique index) ve 2627 (unique constraint) ihlali
    public static bool IsUniqueViolation(this DbUpdateException exception) =>
        exception.InnerException is SqlException { Number: 2601 or 2627 };
}