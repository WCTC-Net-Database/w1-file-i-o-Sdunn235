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
        if (!File.Exists(path))
            throw new FileNotFoundException($"Configuration file not found: {path}");

        var root = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path))
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
