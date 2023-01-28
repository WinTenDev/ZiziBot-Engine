using System.Diagnostics.CodeAnalysis;

namespace ZiziBot.Contracts.Constants;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class UrlConst
{
    public const string OCR_URL_API = "https://api.optiic.dev/process";

    public const string COMBOT_ANTISPAM_API = "https://api.cas.chat/check";
}