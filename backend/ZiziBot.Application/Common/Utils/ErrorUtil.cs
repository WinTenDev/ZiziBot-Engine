namespace ZiziBot.Application.Common.Utils;

public static class ErrorUtil
{
    public static bool IsIgnorable(this string message)
    {
        var ignorableMessages = new List<string>() {
            "bot is not a member",
            "cannot begin with",
            "can't remove chat owner",
            "chat not found",
            "connection attempt failed",
            "connection could not be established",
            "connection refused",
            "error occurred while parsing",
            "error while copying content",
            "forcibly closed by the remote host",
            "invalid uri",
            "message can't be deleted",
            "message content and reply markup are exactly the same",
            "message is not modified",
            "message to delete not found",
            "multiple root elements",
            "name does not resolve",
            "name or service not known",
            "no connection could be made because the target machine actively refused it",
            "no such host is known",
            "not match",
            "reference to undeclared entity",
            "request was canceled",
            "root element is missing",
            "root level is invalid",
            "unexpected token",
            "unknown feed type",
            "user is deactivated"
        };

        return ignorableMessages.Exists(s => message.Contains(s, StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsIgnorable(this Exception exception)
    {
        if (exception.Message.IsIgnorable())
            return true;

        if (exception.InnerException?.Message.IsIgnorable() ?? false)
            return true;

        return false;
    }
}