using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CodeSigningApi.ModelBinders;

public class JsonModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext, nameof(bindingContext));

        // Check the value sent in
        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        
        if (valueProviderResult == ValueProviderResult.None)
            return Task.CompletedTask;
        
        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

        // Attempt to convert the input value
        var valueAsString = valueProviderResult.FirstValue;
        var result = Newtonsoft.Json.JsonConvert.DeserializeObject(valueAsString, bindingContext.ModelType);
       
        if (result == null)
            return Task.CompletedTask;
        
        bindingContext.Result = ModelBindingResult.Success(result);
        return Task.CompletedTask;

    }
}