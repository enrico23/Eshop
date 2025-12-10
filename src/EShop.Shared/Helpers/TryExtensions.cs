namespace EShop.Shared.Helpers;

public static class TryExtensions
{
    public static async Task<Result<T>> Try<T>(Func<Task<T>> func)
    {
        try
        {
            return Result<T>.Success(await func());
        }
        catch (Exception ex)
        {
            return Result<T>.Failure(ex);
        }
    }
}
