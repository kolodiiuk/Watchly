using Microsoft.AspNetCore.Mvc;
using Watchly.Shared.ModelBinders;
using Watchly.Shared.Models;

namespace Watchly.Application.Models.Content;

public sealed record FilterRequest
{
    [ModelBinder(BinderType = typeof(CommaSeparatedArrayModelBinder))]
    public IEnumerable<int> Genres { get; init; }
    [ModelBinder(BinderType = typeof(CommaSeparatedArrayModelBinder))]
    public IEnumerable<int> Keywords { get; set; }
    [ModelBinder(BinderType = typeof(CommaSeparatedArrayModelBinder))]
    public IEnumerable<int> SpokenLanguages { get; set; }
    [ModelBinder(BinderType = typeof(CommaSeparatedArrayModelBinder))]
    public IEnumerable<int> TitleTypes { get; init; }
    [ModelBinder(BinderType = typeof(IntRangeModelBinder))]
    public IntRange YearsRange { get; init; }
    [ModelBinder(BinderType = typeof(FloatRangeModelBinder))]
    public FloatRange RatingRange { get; init; }
}
