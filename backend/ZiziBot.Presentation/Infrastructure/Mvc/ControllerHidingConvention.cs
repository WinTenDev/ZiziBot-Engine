using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace ZiziBot.Presentation.Infrastructure.Mvc;

public class ControllerHidingConvention : IControllerModelConvention
{
	public void Apply(ControllerModel controller)
	{
		if (controller.ControllerName.StartsWith("TelegramController", StringComparison.InvariantCultureIgnoreCase))
		{
			controller.ApiExplorer.IsVisible = false;
		}
	}
}

