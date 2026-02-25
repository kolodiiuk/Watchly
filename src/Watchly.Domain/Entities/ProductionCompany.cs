namespace Watchly.Domain.Entities;

public sealed class ProductionCompany
{
    public int Id { get; set; }

    public string Name { get; set; }

    public ICollection<TitleProductionCompany> TitleProductionCompanies { get; set; }
        = new List<TitleProductionCompany>();
}
