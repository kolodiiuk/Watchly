using System.Text.Json.Serialization;

namespace Watchly.Domain.Entities;

public sealed class Keyword
{
    public int Id { get; set; }

    public string Name { get; set; }

    [JsonIgnore]
    public ICollection<KeywordTitle> KeywordTitles { get; set; } = new List<KeywordTitle>();
}
