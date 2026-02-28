namespace Watchly.Domain.Utils;

public readonly record struct ResultV<TErrorCode>(Result Result, TErrorCode ErrorCode);
