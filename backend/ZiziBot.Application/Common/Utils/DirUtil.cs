using System.IO.Compression;
using Serilog;

namespace ZiziBot.Application.Common.Utils;

public static class DirUtil
{
    public static string EnsureDirectory(this string path)
    {
        var directory = Path.GetDirectoryName(path);

        if (Directory.Exists(directory)) return path;
        if (string.IsNullOrEmpty(directory)) return path;

        Directory.CreateDirectory(directory);

        return path;
    }

    public static string DeleteDirectory(this string path)
    {
        try
        {
            Directory.Delete(path, true);
        }
        catch (Exception e)
        {
            Log.Debug(e, "Failed to delete directory: {Path} with error: {Message}", path, e.Message);
        }

        return path;
    }

    public static string DeleteFile(this string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (Exception e)
        {
            Log.Debug(e, "Failed to delete directory: {Path} with error: {Message}", path, e.Message);
        }

        return path;
    }

    public static int DeleteFile(this List<FileInfo> listFile)
    {
        foreach (var fileInfo in listFile)
        {
            Log.Debug("Delete file: {FullName}", fileInfo.FullName);

            fileInfo.Delete();
        }

        return listFile.Count;
    }

    public static string GetCurrentDirectory(this string path)
    {
        var dirName = new DirectoryInfo(path).Name;
        return dirName;
    }

    public static string GetFileName(this string? path)
    {
        if (path.IsNullOrWhiteSpace())
            return string.Empty;

        return Path.GetFileName(path);
    }

    public static string CompressToZip(this string dirSource, string? fileName = null, bool deletePrev = true)
    {
        var zipFileName = dirSource.TrimEnd('/') + ".zip";
        if (fileName != null)
        {
            var dirName = dirSource.GetCurrentDirectory();
            zipFileName = Path.Combine(dirSource.Replace(dirName, ""), fileName);
        }

        if (deletePrev)
            File.Delete(zipFileName);

        ZipFile.CreateFromDirectory(dirSource, zipFileName);

        return zipFileName;
    }

    public static List<FileInfo> GetFiles(this string dirPath, string pattern = "*.*", Func<FileInfo, bool>? predicate = null)
    {
        return Directory.EnumerateFiles(dirPath, pattern, SearchOption.AllDirectories).Select(x => new FileInfo(x))
            .WhereIf(predicate != null, predicate)
            .ToList();
    }

    /// <summary>
    /// Recursively deletes all empty directories under the specified path
    /// </summary>
    /// <param name="path">The root directory path to search for empty directories</param>
    /// <param name="deleteRootIfEmpty">Whether to delete the root directory if it becomes empty</param>
    /// <returns>List of directories that were deleted</returns>
    public static List<string> DeleteEmptyDirectories(this string path, bool deleteRootIfEmpty = true)
    {
        var deletedDirectories = new List<string>();

        try
        {
            // Skip if directory doesn't exist
            if (!Directory.Exists(path))
                return deletedDirectories;

            // Recursively process subdirectories
            foreach (var directory in Directory.GetDirectories(path))
            {
                deletedDirectories.AddRange(directory.DeleteEmptyDirectories());
            }

            // Check if current directory is empty
            var files = Directory.GetFiles(path);
            var subDirs = Directory.GetDirectories(path);

            // If directory is empty and either it's not the root or we want to delete the root if empty
            if (files.Length == 0 && subDirs.Length == 0 && (deleteRootIfEmpty || path != Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar)))
            {
                try
                {
                    Directory.Delete(path, false);
                    deletedDirectories.Add(path);
                    Log.Debug("Deleted empty directory: {Path}", path);
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Failed to delete directory: {Path}", path);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while deleting empty directories under: {Path}", path);
        }

        return deletedDirectories;
    }
}