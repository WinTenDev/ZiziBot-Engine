using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using ZiziBot.Attributes;

namespace ZiziBot.Presentation.Infrastructure.Swagger;

public class SwaggerIgnoreFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema.Properties == null || schema.Properties.Count == 0)
        {
            return;
        }

        var properties = context.Type.GetProperties();
        var excludedList = properties
            .Where(m => m.GetCustomAttribute<SwaggerIgnoreAttribute>() is not null)
            .Select(m => m.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? m.Name.ToCamelCase());

        foreach (var excludedName in excludedList)
        {
            schema.Properties.Remove(excludedName);
        }
    }
}
