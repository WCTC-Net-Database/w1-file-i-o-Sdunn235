using System.Text.Json;

namespace ConsoleRpg.Helpers;

/// <summary>
/// Loads typed configuration from appsettings.json.
/// Provides path values to Startup so no raw strings exist anywhere in service code.
///
/// SRP: Configuration loading is its own isolated responsibility.
/// DIP: Startup depends on DataFileConfig (typed object), never on raw path strings.
/// </summary>
public static class ConfigurationHelper
{
    public static DataFileConfig LoadDataFileConfig(string path = "appsettings.json")
    {
        // Resolve relative paths against the build output directory, not the
        // current working directory. This makes the app work whether launched
        // from the solution root, the ConsoleRpg/ folder, or via an IDE.
        var resolvedPath = Path.IsPathRooted(path)
            ? path
            : Path.Combine(AppContext.BaseDirectory, path);

        if (!File.Exists(resolvedPath))
            throw new FileNotFoundException($"Configuration file not found: {resolvedPath}");

        var root = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(resolvedPath))
            ?? throw new InvalidOperationException("Failed to deserialize appsettings.json.");

        return root.DataFiles;
    }
}

public class AppSettings
{
    public DataFileConfig DataFiles { get; set; } = new();
}

public class DataFileConfig
{
    public string Players { get; set; } = string.Empty;
    public string Monsters { get; set; } = string.Empty;
    public string Items { get; set; } = string.Empty;
}
