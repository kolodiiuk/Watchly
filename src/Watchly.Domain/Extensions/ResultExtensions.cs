using Watchly.Domain.Utils;

namespace Watchly.Domain.Extensions;

public static class ResultExtensions
{
    public static Result<TType, TError> WithError<TType, TError>(this Result<TType> result, TError error)
    {
        return new Result<TType, TError>(result, error);
    }

    public static ResultV<TError> WithError<TError>(this Result result, TError error)
    {
        return new ResultV<TError>(result, error);
    }
}
