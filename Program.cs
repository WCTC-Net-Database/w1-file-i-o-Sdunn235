using W5SolidLsp.Interfaces;
using W5SolidLsp.Models.Characters.Npcs.Monsters;
using W5SolidLsp.Models.Classes;
using W5SolidLsp.Services;

/// <summary>
/// Week 5: LSP & ISP - Console RPG Character Manager
///
/// This program demonstrates the Liskov Substitution Principle (LSP) and
/// Interface Segregation Principle (ISP), building on Week 4's OCP work:
/// - IEntity: Clean base interface (no Fly - that violated LSP!)
/// - IFlyable, IShootable, ISwimmable: Focused behavior interfaces (ISP)
/// - GameEngine: Processes entities using the 'is' keyword to check capabilities
/// - IFileHandler: Interface for all file operations (carried over from W4)
///
/// The key LSP fix: Fly() is no longer on IEntity. Only entities that CAN fly
/// implement IFlyable. GameEngine checks before calling - no NotSupportedException!
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
    /// Demonstrates the W5 GameEngine with LSP + ISP in action.
    /// Creates a mixed list of entities and processes each one.
    /// The GameEngine only knows about IEntity — it uses 'is' to discover
    /// optional capabilities at runtime without breaking LSP.
    /// </summary>
    private static void RunGameEngineDemo()
    {
        Console.WriteLine("=== W5: GameEngine Demo (LSP + ISP) ===\n");

        // Build a list of mixed entities — GameEngine only sees IEntity
        var entities = new List<IEntity>
        {
            new Ghost("Shade", 3, 20),
            new Goblin("Gruk", 1, 15, "crude dagger"),
            new Troll("Morg", 4, 60, "club"),
            new Archer("Robin", 2, 30, "longbow|quiver")
        };

        var engine = new GameEngine(entities);

        // Run using direct 'is' checks (Tasks 2 & 3)
        Console.WriteLine("--- Direct processing (is keyword) ---");
        engine.RunTurn();

        // Run using Command Pattern (Stretch Goal)
        Console.WriteLine("--- Command queue (Command Pattern stretch goal) ---");
        engine.RunTurnWithCommands();

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
