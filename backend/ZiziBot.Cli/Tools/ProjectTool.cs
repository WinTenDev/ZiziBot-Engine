using System.Text.Json;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using ZiziBot.Application.Common.Types;

namespace ZiziBot.Cli.Tools;

/*
 * Reference: https://github.com/TAGC/dotnet-setversion/blob/develop/src/dotnet-setversion/Program.cs
 * Copyright (c) ThymineC
 */

public class ProjectTool(ILogger<ProjectTool> logger)
{
    internal const int ExitSuccess = 0;
    internal const int ExitFailure = 1;
    internal static bool UseVersionPrefix = false;

    public void UpdateProjectVersion(string mode)
    {
        var buildProps = "Directory.Build.props";
        var baseDirectory = Directory.GetCurrentDirectory();
        var jan2000 = new DateTime(2000, 1, 1);
        var currentDate = DateTime.Now;

        var majorNumber = currentDate.Year.ToString().Replace("0", "");
        var minorNumber = currentDate.Month;
        var buildNumber = (currentDate - jan2000).Days;
        var revNumber = TimeSpan.Parse(currentDate.ToString("h:mm:ss")).TotalSeconds;
        var projectVersion = $"{majorNumber}.{minorNumber}.{buildNumber}.{revNumber}";

        // Environment.SetEnvironmentVariable("VERSION_NUMBER", projectVersion);

        switch (mode)
        {
            case "DependsOnCondition":
                if (File.Exists(buildProps))
                {
                    SetVersion(projectVersion, buildProps);
                }
                else
                {
                    RunRecursive(baseDirectory: baseDirectory, version: projectVersion);
                }

                break;
            case "RootOnly":
                SetVersion(projectVersion, buildProps);
                break;
            case "RootAndAllProjects":
                SetVersion(projectVersion, buildProps);
                RunRecursive(baseDirectory: baseDirectory, version: projectVersion);
                break;
            case "AllProjectsOnly":
                RunRecursive(baseDirectory: baseDirectory, version: projectVersion);
                break;
            default:
                break;
        }

        // var envVersionNumber = Environment.GetEnvironmentVariable("VERSION_NUMBER");
        logger.LogInformation("Project version updated to {ProjectVersion}", projectVersion);
        // logger.LogInformation("Environment variable VERSION_NUMBER set to {EnvVersionNumber}", envVersionNumber);
    }

    private int RunRecursive(
        string baseDirectory,
        string version
    )
    {
        var csprojFiles = GetCsprojFiles(baseDirectory, recursive: true);
        if (!CheckCsprojFiles(csprojFiles, allowMultiple: true))
            return ExitFailure;

        if (ShouldExtractVersionFromFile(version, out var versionFile) &&
            !TryExtractVersionFromFile(versionFile, out version))
        {
            return ExitFailure;
        }

        foreach (var csprojFile in csprojFiles)
        {
            SetVersion(version, csprojFile);
        }

        PrintSuccessString(version, csprojFiles);
        return ExitSuccess;
    }

    private string[] GetCsprojFiles(
        string baseDirectory,
        bool recursive
    )
    {
        var projectFiles = Directory
            .EnumerateFileSystemEntries(
                path: baseDirectory,
                searchPattern: "*.csproj",
                searchOption: recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly
            ).ToArray();

        logger.LogInformation("Found {Count} project files", projectFiles.Length);

        return projectFiles;
    }

    private static bool CheckCsprojFiles(
        string[] csprojFiles,
        bool allowMultiple
    )
    {
        if (csprojFiles.Length == 0)
        {
            Console.WriteLine("Specify a project file. The current working directory does not contain a project file.");
            return false;
        }

        if (!allowMultiple &&
            csprojFiles.Length > 1)
        {
            Console.WriteLine("Specify which project file to use because this folder contains more than one project file.");
            return false;
        }

        return true;
    }

    private static bool ShouldExtractVersionFromFile(
        string version,
        out string versionFile
    )
    {
        if (version.StartsWith("@"))
        {
            versionFile = version.Substring(1);
            return true;
        }

        versionFile = null;
        return false;
    }

    private static bool TryExtractVersionFromFile(
        string filename,
        out string version
    )
    {
        if (!File.Exists(filename))
        {
            Console.WriteLine($"The specified file to extract the version from was not found: {filename}");
            version = null;
            return false;
        }

        var versionFileText = File.ReadAllText(filename).Trim();

        try
        {
            var versionModel = JsonSerializer.Deserialize<VersionModel>(versionFileText);
            version = versionModel.ToString();
        }
        catch (JsonException)
        {
            // Simple Version Number
            version = versionFileText;
        }

        return true;
    }

    private void SetVersion(
        string version,
        string csprojFile
    )
    {
        if (version == null) throw new ArgumentNullException(nameof(version));
        if (csprojFile == null) throw new ArgumentNullException(nameof(csprojFile));

        logger.LogInformation("Updating {CsprojFile} to Version {Version}", csprojFile, version);

        var versionElement = UseVersionPrefix ? "VersionPrefix" : "Version";

        var document = XDocument.Load(csprojFile);
        var projectNode = document.GetOrCreateElement("Project");
        var versionNode = projectNode
                              .Elements("PropertyGroup")
                              .SelectMany(it => it.Elements(versionElement))
                              .SingleOrDefault() ??
                          projectNode
                              .GetOrCreateElement("PropertyGroup")
                              .GetOrCreateElement(versionElement);

        versionNode.SetValue(version);
        File.WriteAllText(csprojFile, document.ToString());
    }

    private static void PrintSuccessString(
        string version,
        string file
    )
    {
        Console.WriteLine($"Set version to {version} in {file}");
    }

    private void PrintSuccessString(
        string version,
        params string[] files
    )
    {
        logger.LogInformation("Set version to {Version} in:", version);

        foreach (var file in files)
        {
            logger.LogInformation("==> {File}", file);
        }
    }
}