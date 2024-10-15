using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Converters
{
    public class ListBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));

            var modelName = bindingContext.ModelName;
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);
            bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);
            var value = valueProviderResult.FirstValue;

            if (string.IsNullOrEmpty(value))
            {
                bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            //if (!int.TryParse(value, out var id))
            //{
            //    // Non-integer arguments result in model state errors
            //    bindingContext.ModelState.TryAddModelError(
            //        modelName, "Author Id must be an integer.");

            //    return Task.CompletedTask;
            //}

            var model = value?.Split(",").ToList();
            bindingContext.Result = ModelBindingResult.Success(model);
            return Task.CompletedTask;
        }
    }
}