using Microsoft.AspNetCore.Mvc.ModelBinding;
using Watchly.Shared.Models;

namespace Watchly.Shared.ModelBinders;

public class FloatRangeModelBinder : IModelBinder
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

        var startParseRes = float.TryParse(split[0], out var start);
        var endParseRes = float.TryParse(split[1], out var end);
        if (!startParseRes)
        {
            start = float.MinValue;
        }

        if (!endParseRes)
        {
            end = float.MaxValue;
        }

        var result = new FloatRange { Start = start, End = end };
        bindingContext.Result = ModelBindingResult.Success(result);

        return Task.CompletedTask;
    }
}
