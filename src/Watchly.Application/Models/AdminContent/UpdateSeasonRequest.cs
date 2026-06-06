namespace Watchly.Application.Models.AdminContent;

public sealed record UpdateSeasonRequest(
    int OrdinalNumber,
    string Name
);
