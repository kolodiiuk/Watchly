using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Watchly.Shared.ModelBinders;

public class CommaSeparatedArrayModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).FirstValue;
        if (string.IsNullOrEmpty(value))
        {
            return Task.CompletedTask;
        }

        var values = value.Split(',', StringSplitOptions.RemoveEmptyEntries);
        var res = new int[values.Length];
        for (int i = 0; i < values.Length; ++i)
        {
            int.TryParse(values[i], out res[i]);
        }

        bindingContext.Result = ModelBindingResult.Success(res);

        return Task.CompletedTask;
    }
}
