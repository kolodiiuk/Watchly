namespace Watchly.Domain.Utils;

public readonly record struct Result<T>
{
    public T Value { get; init; }

    public bool IsSuccess { get; }

    public string Error { get; }

    public bool Failure => !IsSuccess;

    public Result(T value, bool isSuccess, string error)
    {
        if (isSuccess && !string.IsNullOrWhiteSpace(error))
        {
            throw new InvalidOperationException(
                "Error can't be filled in the case of success.");
        }

        if (!isSuccess && string.IsNullOrWhiteSpace(error))
        {
            throw new InvalidOperationException(
                "Error can't be empty in the case of failure.");
        }

        IsSuccess = isSuccess;
        Error = error;
        Value = value;
    }

    public static Result<T> Fail(string message)
    {
        return new Result<T>(default(T), false, message);
    }

    public static Result<T> Success(T value)
    {
        return new Result<T>(value, true, string.Empty);
    }
}
