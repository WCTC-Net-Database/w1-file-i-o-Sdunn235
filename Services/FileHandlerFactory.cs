using W6DependencyInversion.Interfaces;

namespace W6DependencyInversion.Services;

/// <summary>
/// Factory that creates IFileHandler implementations by format name.
/// Applying DIP: Program.cs depends on IFileHandler (abstraction) and this factory
/// instead of directly constructing CsvFileHandler or JsonFileHandler (concretions).
///
/// Adding a new format (XML, Database, etc.) requires only a new case here -
/// no changes needed in Program.cs or any consumer of IFileHandler.
/// </summary>
public static class FileHandlerFactory
{
    /// <summary>
    /// Creates the appropriate IFileHandler for the given format name.
    /// </summary>
    /// <param name="format">Format name: "csv" or "json" (case-insensitive).</param>
    /// <returns>An IFileHandler implementation for the requested format.</returns>
    /// <exception cref="ArgumentException">Thrown when the format is not supported.</exception>
    public static IFileHandler Create(string format) =>
        format.ToLower() switch
        {
            "csv"  => new CsvFileHandler("Input/input.csv"),
            "json" => new JsonFileHandler("Input/input.json"),
            _ => throw new ArgumentException($"Unsupported file format: '{format}'. Supported formats: csv, json.")
        };
}
