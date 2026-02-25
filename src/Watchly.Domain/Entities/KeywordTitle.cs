namespace Watchly.Domain.Entities;

public sealed class KeywordTitle
{
    public int Id { get; set; }

    public int TitleId { get; set; }
    
    public int KeywordId { get; set; }

    public Title Title { get; set; }

    public Keyword Keyword { get; set; }
}
