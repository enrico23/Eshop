using Microsoft.Data.SqlClient;

namespace EShop.Shared.Exceptions;

public static class SqlServerExceptionsHelper
{
    public static bool IsUniqueViolation(Exception ex)
       => ex.InnerException is SqlException sqlEx
          && (sqlEx.Number == 2601 || sqlEx.Number == 2627);
}
