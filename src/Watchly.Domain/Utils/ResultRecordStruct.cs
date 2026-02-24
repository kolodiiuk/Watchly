namespace Watchly.Domain.Utils;

public readonly record struct Result
{
    public bool IsSuccess { get; }

    public string Error { get; }

    public bool Failure => !IsSuccess;

    private Result(bool isSuccess, string error)
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
    }

    public static Result Fail(string message)
    {
        return new Result(false, message);
    }

    public static Result Success()
    {
        return new Result(true, string.Empty);
    }
}
