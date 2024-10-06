using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ZiziBot.WebApi.Providers;

public class PathPrefixSwaggerDocumentFilter(string prefix) : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
	{
		var paths = swaggerDoc.Paths.Keys.ToList();

		paths.ForEach(
			path => {
				var pathToChange = swaggerDoc.Paths[path];
				swaggerDoc.Paths.Remove(path);
				swaggerDoc.Paths.Add(prefix + path, pathToChange);
			}
		);
	}
}