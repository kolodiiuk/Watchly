namespace Watchly.Domain.Utils;

public readonly record struct Result<TType, TError>(Result<TType> WrappedResult, TError Error);
