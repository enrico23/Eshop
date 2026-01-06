using Microsoft.Data.SqlClient;

namespace EShop.Shared.Exceptions;

public sealed class SqlServerExceptionsHelper : ISqlServerExceptionsHelper
{
    public bool IsUniqueViolation(Exception ex)
       => ex.InnerException is SqlException sqlEx
          && (sqlEx.Number == 2601 || sqlEx.Number == 2627);
}

public interface ISqlServerExceptionsHelper
{
    bool IsUniqueViolation(Exception ex);
}