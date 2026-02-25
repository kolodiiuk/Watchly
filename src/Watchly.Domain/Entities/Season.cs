namespace Watchly.Domain.Entities;

public sealed class Season
{
    public int Id { get; set; }

    public int OrdinalNumber { get; set; }
    
    public string Name { get; set; }
    
    public int TitleId { get; set; }

    public Title Title { get; set; }

    public ICollection<Episode> Episodes { get; set; } = new List<Episode>();
}
