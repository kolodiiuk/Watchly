namespace Watchly.Domain.Entities;

public sealed class TitleSpokenLanguage
{
    public int Id { get; set; }

    public int SpokenLanguageId { get; set; }
    
    public int TitleId { get; set; }

    public SpokenLanguage SpokenLanguage { get; set; }

    public Title Title { get; set; }
}
