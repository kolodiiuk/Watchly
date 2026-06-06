namespace Watchly.Application.Models.AdminContent;

public sealed record TitleReferenceOption(int Id, string Name);

public sealed record TitleReferenceOptions(
    IEnumerable<TitleReferenceOption> Genres,
    IEnumerable<TitleReferenceOption> SpokenLanguages,
    IEnumerable<TitleReferenceOption> ProductionCompanies);
