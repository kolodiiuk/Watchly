namespace Watchly.Domain.Entities;

public sealed class TitleProductionCompany
{
    public int Id { get; set; }

    public int TitleId { get; set; }

    public bool IsTvShow { get; set; }
    
    public int ProductionCompanyId { get; set; }

    public ProductionCompany ProductionCompany { get; set; }

    public Title Title { get; set; }
}
