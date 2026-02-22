using W4Ocp.Interfaces;
using W4Ocp.Services;

/// <summary>
/// Week 4: Open/Closed Principle - Console RPG Character Manager
///
/// This program demonstrates the Open/Closed Principle (OCP):
/// - IFileHandler: Interface for all file operations
/// - CsvFileHandler: CSV implementation
/// - JsonFileHandler: JSON implementation (can be swapped without changing business logic!)
/// - CharacterUI: Handles console interactions (doesn't care about file format)
/// - MenuService: Handles menu display and navigation
/// - Program: Orchestrates the application flow (main loop only)
///
/// The key: Program uses IFileHandler, not a concrete class. This means we can swap
/// CSV for JSON (or add XML, Database, etc.) without modifying existing code!
///
/// STRETCH GOAL: Runtime format switching - Change between CSV and JSON without restarting!
/// </summary>
///
/// <remarks>
/// *Disclosure*: This project was initially set up using AI assistance as part of a Week 2 learning exercise.
/// The code has since been reviewed, tested, and modified to ensure it works as intended.
/// Moving forward, testing and modifications will be self-directed.
/// </remarks>

class Program
{
    // Track current format and handlers at the class level
    private static IFileHandler _fileHandler = new CsvFileHandler("Input/input.csv");
    private static string _currentFormat = "CSV";
    private static CharacterUI _characterUI = null!;
    private static MenuService _menu = null!;

    static void Main()
    {
        // Initialize services
        _characterUI = new CharacterUI(_fileHandler);
        _menu = new MenuService();

        // Display welcome message
        _menu.DisplayWelcome();

        // Main program loop - keeps running until user chooses to exit
        bool running = true;
        while (running)
        {
            // Display current format in the menu
            Console.WriteLine($"[Current Format: {_currentFormat}]");

            // Display the menu and get user's choice
            _menu.DisplayMainMenu();
            string choice = _menu.GetMenuChoice();

            // Process the user's choice using a switch statement
            switch (choice)
            {
                case "1":
                    _characterUI.DisplayAllCharacters();
                    break;
                case "2":
                    _characterUI.FindCharacter();
                    break;
                case "3":
                    _characterUI.AddCharacter();
                    break;
                case "4":
                    _characterUI.LevelUpCharacter();
                    break;
                case "5":
                    SwitchFileFormat();
                    break;
                case "0":
                    running = false;
                    _menu.DisplayGoodbye();
                    break;
                default:
                    _menu.DisplayInvalidChoice();
                    break;
            }

            // Pause and clear screen between operations
            if (running)
            {
                _menu.PauseAndClear();
            }
        }
    }

    /// <summary>
    /// Handles switching between file formats (CSV and JSON).
    /// Demonstrates the Open/Closed Principle - we can add new formats without modifying existing code!
    /// </summary>
    private static void SwitchFileFormat()
    {
        string selectedFormat = _menu.GetFileFormatChoice();

        // User cancelled the operation
        if (string.IsNullOrEmpty(selectedFormat))
        {
            _menu.DisplayFormatChangeCancelled();
            return;
        }

        // Create the appropriate handler based on user choice
        // Notice: We can easily add more formats here (XML, YAML, Database, etc.)
        // without modifying any existing code - that's OCP in action!
        _fileHandler = selectedFormat.ToLower() switch
        {
            "csv" => new CsvFileHandler("Input/input.csv"),
            "json" => new JsonFileHandler("Input/input.json"),
            _ => _fileHandler // Fallback to current handler
        };

        // Update the format name and recreate CharacterUI with new handler
        _currentFormat = selectedFormat.ToUpper();
        _characterUI = new CharacterUI(_fileHandler);

        _menu.DisplayFormatChanged(_currentFormat);
    }
}
