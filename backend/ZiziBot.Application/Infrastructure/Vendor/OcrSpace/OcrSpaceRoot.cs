namespace ZiziBot.Application.Infrastructure.Vendor.OcrSpace;

public class OcrSpaceRoot
{
    public List<ParsedResult>? ParsedResults { get; set; }
    public int OcrExitCode { get; set; }
    public bool IsErroredOnProcessing { get; set; }
    public int ProcessingTimeInMilliseconds { get; set; }
    public string SearchablePdfurl { get; set; }
}

public class ParsedResult
{
    public TextOverlay TextOverlay { get; set; }
    public int TextOrientation { get; set; }
    public int FileParseExitCode { get; set; }
    public string ParsedText { get; set; }
    public string ErrorMessage { get; set; }
    public string ErrorDetails { get; set; }
}

public class TextOverlay
{
    public List<object> Lines { get; set; }
    public bool HasOverlay { get; set; }
    public string Message { get; set; }
}