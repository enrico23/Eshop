namespace EShop.Shared.Helpers;

public readonly struct Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public Exception? Error { get; }

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
        Error = null;
    }

    private Result(Exception ex)
    {
        IsSuccess = false;
        Error = ex;
        Value = default;
    }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(Exception ex) => new(ex);

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Exception, TResult> onFailure)
        => IsSuccess ? onSuccess(Value!) : onFailure(Error!);
}