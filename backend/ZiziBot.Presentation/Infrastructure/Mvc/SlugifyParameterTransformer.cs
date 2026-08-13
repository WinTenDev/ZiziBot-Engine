using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Routing;

namespace ZiziBot.Presentation.Infrastructure.Mvc;

public class SlugifyParameterTransformer : IOutboundParameterTransformer
{

	public string? TransformOutbound(object? value)
	{
		if (value == null) { return null; }

		return Regex.Replace(
			input: value.ToString(),
			pattern: "([a-z])([A-Z])",
			replacement: "$1-$2",
			options: RegexOptions.CultureInvariant,
			matchTimeout: TimeSpan.FromMilliseconds(100)
		).ToLowerInvariant();
	}
}

