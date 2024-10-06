namespace ZiziBot.Utils;

public static class ErrorUtil
{
    public static bool IsIgnorable(this string message)
    {
        var ignorableMessages = new List<string>() {
            "bot is not a member",
            "bot was blocked",
            "cannot begin with",
            "chat not found",
            "connection attempt failed",
            "connection could not be established",
            "error occurred while parsing",
            "error while copying content",
            "forcibly closed by the remote host",
            "message can't be deleted",
            "message to delete not found",
            "multiple root elements",
            "name does not resolve",
            "name or service not known",
            "no such host is known",
            "not match",
            "reference to undeclared entity",
            "request was canceled",
            "root element is missing",
            "root level is invalid",
            "unexpected token",
            "unknown feed type",
            "user is deactivated",
        };

        return ignorableMessages.Exists(message.ToLower().Contains);
    }

    public static bool IsRssBetterDisabled(this Exception exception)
    {
        if (exception.Message.IsIgnorable())
            return true;

        if (exception.InnerException?.Message.IsIgnorable() ?? false)
            return true;

        return false;
    }
}