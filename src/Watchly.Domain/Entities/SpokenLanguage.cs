namespace Watchly.Domain.Entities;

public sealed class SpokenLanguage
{
    public int Id { get; set; }

    public string Name { get; set; }

    public ICollection<TitleSpokenLanguage> TitleSpokenLanguages { get; set; } = new List<TitleSpokenLanguage>();
}
