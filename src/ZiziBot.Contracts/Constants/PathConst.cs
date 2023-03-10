using System.Diagnostics.CodeAnalysis;

namespace ZiziBot.Contracts.Constants;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class PathConst
{
    public const string TEMP_PATH = "Storage/Temp/";

    public static readonly string CACHE_TOWER_PATH = "Storage/CacheTower/File/".EnsureDirectory();
}