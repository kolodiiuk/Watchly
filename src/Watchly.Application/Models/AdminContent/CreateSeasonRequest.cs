namespace Watchly.Application.Models.AdminContent;

public sealed record CreateSeasonRequest(
    int OrdinalNumber,
    string Name
);
