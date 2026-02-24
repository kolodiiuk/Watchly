namespace Watchly.Domain.Utils;

public readonly record struct ResultV<TError>(Result Result, TError Error);
