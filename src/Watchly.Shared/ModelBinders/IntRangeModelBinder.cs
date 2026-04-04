using Microsoft.AspNetCore.Mvc.ModelBinding;
using Watchly.Shared.Models;

namespace Watchly.Shared.ModelBinders;

public class IntRangeModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).FirstValue;
        if (string.IsNullOrEmpty(value))
        {
            return Task.CompletedTask;
        }

        var split = value.Split("-");
        if (split.Length != 2)
        {
            return Task.CompletedTask;
        }

        var startParseRes = int.TryParse(split[0], out var start);
        var endParseRes = int.TryParse(split[1], out var end);
        if (!startParseRes)
        {
            start = int.MinValue;
        }

        if (!endParseRes)
        {
            end = int.MaxValue;
        }

        var result = new IntRange { Start = start, End = end };
        bindingContext.Result = ModelBindingResult.Success(result);

        return Task.CompletedTask;
    }
}
