using Watchly.Domain.Enums;
using Watchly.Shared.Models;

namespace Watchly.Application.Models;

public sealed record FilterRequest
{
    public IEnumerable<string> Genres { get; init; }
    
    public IntRange YearsRange { get; init; }
    
    public TitleType TitleTypes { get; init; }
    
    public FloatRange RatingRange { get; init; }
}
