using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Converters
{
    public class QueryArrayParameterFilter : IParameterFilter
    {
        public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
        {
            if (!parameter.In.HasValue || parameter.In.Value != ParameterLocation.Query)
                return;

            var converterAttribute = context.ParameterInfo.GetCustomAttribute<ModelBinderAttribute>();

            if (converterAttribute != null && parameter.Schema?.Type == "array" && parameter.Name.Equals("communityEntityIds"))
            {
                parameter.Schema.Type = "string";
                parameter.Style = ParameterStyle.Simple;
                parameter.Schema.Items = null;
            }
        }
    }
}
