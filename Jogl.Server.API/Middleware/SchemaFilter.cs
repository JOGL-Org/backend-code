using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Middleware
{
    public class SchemaFilter : ISchemaFilter
    {
        private NullabilityInfoContext _nullabilityContext = new NullabilityInfoContext();


        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            //hack to filter out System.String, List<> etc
            if (!context.Type.Assembly.FullName.StartsWith("Jogl"))
                return;

            var type = context.Type.FullName;

            var nonNullableProps = context.Type.GetProperties()
                .Where(x => x.GetCustomAttribute<JsonIgnoreAttribute>() == null)
                .Where(x => _nullabilityContext.Create(x).WriteState != NullabilityState.Nullable)
                .ToList();

            schema.Required = nonNullableProps.Select(x => x.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? x.Name.ToLower()).ToHashSet();
        }
    }
}