using W6SolidDip.Interfaces;
using W6SolidDip.Models.Characters.Npcs.Monsters;
using W6SolidDip.Models.Classes;
using W6SolidDip.Services;

/// <summary>
/// Week 6: DIP & Abstract Classes - Console RPG Character Manager
///
/// This program demonstrates the Dependency Inversion Principle (DIP) and
/// Abstract Classes, building on Week 5's LSP/ISP work:
/// - CharacterBase: Abstract class with shared code and abstract PerformSpecialAction
/// - GameEngine: Depends on IFileHandler and IEntity (abstractions), not concretions
/// - Dependencies injected into GameEngine via constructor (DIP)
/// - All character classes override PerformSpecialAction with unique behavior
///
/// DIP key point: GameEngine never says "new CsvFileHandler" or "new Ghost".
/// Concrete types are created here in Program (the composition root) and injected.
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

        // --- W5: Run the GameEngine demo before the main menu ---
        RunGameEngineDemo();

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
    /// Demonstrates W6 GameEngine with DIP and Abstract Classes.
    /// GameEngine depends on abstractions (IEntity, IFileHandler), not concrete types.
    /// Dependencies are injected via constructor, demonstrating Dependency Inversion Principle.
    /// All entities implement abstract PerformSpecialAction method from CharacterBase.
    /// </summary>
    private static void RunGameEngineDemo()
    {
        Console.WriteLine("=== W6: GameEngine Demo (DIP + Abstract Classes) ===\n");

        // DIP: Create a mixed list of entities — GameEngine only sees IEntity (abstraction)
        var entities = new List<IEntity>
        {
            new Ghost("Shade", 3, 20),
            new Goblin("Gruk", 1, 15, "crude dagger"),
            new Troll("Morg", 4, 60, "club"),
            new Archer("Robin", 2, 30, "longbow|quiver"),
            new Healer("Mira", 2, 18, "staff|bandages"),
            new Paladin("Aldric", 3, 32, "sword|shield|holy symbol")
        };

        // DIP: Inject IFileHandler and List<IEntity> — GameEngine depends on abstractions, not concretions
        var engine = new GameEngine(_fileHandler, entities);

        // Process all entities - demonstrates abstract PerformSpecialAction being called polymorphically
        engine.RunTurn();

        Console.WriteLine("\nPress any key to continue to the Character Manager...");
        Console.ReadKey();
        Console.Clear();
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
