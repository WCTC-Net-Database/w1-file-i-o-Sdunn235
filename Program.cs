using W6DependencyInversion.Interfaces;
using W6DependencyInversion.Models.Characters.Npcs.Monsters;
using W6DependencyInversion.Models.Classes;
using W6DependencyInversion.Services;

/// <summary>
/// Week 6: Dependency Inversion Principle (DIP)
///
/// Key W6 changes from W5:
/// - CharacterBase: abstract base with PerformSpecialAction() contract
/// - CsvHelper moved out of Models - no more CsvHelper dependency in Character
/// - IFileHandler updated to use CharacterBase (abstraction over concretion)
/// - FileHandlerFactory: Program.cs depends on IFileHandler abstraction (DIP)
/// - GameEngine calls PerformSpecialAction() via CharacterBase (DIP)
/// - New classes: Necromancer (raises undead), Paladin (lays on hands)
/// </summary>
///
/// <remarks>
/// *Disclosure*: This project was initially set up using AI assistance as part of a Week 2 learning exercise.
/// The code has since been reviewed, tested, and modified to ensure it works as intended.
/// Moving forward, testing and modifications will be self-directed.
/// </remarks>

class Program
{
    // DIP: depend on the IFileHandler abstraction, not CsvFileHandler/JsonFileHandler concretions.
    // FileHandlerFactory creates the concrete implementation - Program.cs never references it.
    private static IFileHandler _fileHandler = FileHandlerFactory.Create("csv");
    private static string _currentFormat = "CSV";
    private static CharacterUI _characterUI = null!;
    private static MenuService _menu = null!;

    static void Main()
    {
        _characterUI = new CharacterUI(_fileHandler);
        _menu = new MenuService();

        _menu.DisplayWelcome();

        // --- W6: Run the GameEngine demo ---
        RunGameEngineDemo();

        bool running = true;
        while (running)
        {
            Console.WriteLine($"[Current Format: {_currentFormat}]");

            _menu.DisplayMainMenu();
            string choice = _menu.GetMenuChoice();

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

            if (running)
            {
                _menu.PauseAndClear();
            }
        }
    }

    /// <summary>
    /// W6 GameEngine demo: demonstrates CharacterBase.PerformSpecialAction() (DIP)
    /// alongside the W5 LSP + ISP interfaces.
    /// Includes the two new stretch-goal classes: Necromancer and Paladin.
    /// </summary>
    private static void RunGameEngineDemo()
    {
        Console.WriteLine("=== W6: GameEngine Demo (DIP + CharacterBase) ===\n");

        // GameEngine depends only on IEntity (DIP).
        // It also calls PerformSpecialAction() when the entity is a CharacterBase.
        var entities = new List<IEntity>
        {
            new Ghost("Shade", 3, 20),
            new Goblin("Gruk", 1, 15, "crude dagger"),
            new Troll("Morg", 4, 60, "club"),
            new Archer("Robin", 2, 30, "longbow|quiver"),
            new Necromancer("Malachar", 3, 14, "skull staff|dark robe"),   // W6 stretch
            new Paladin("Seraphina", 2, 28, "holy sword|plate armor")      // W6 stretch
        };

        var engine = new GameEngine(entities);

        Console.WriteLine("--- Direct processing (is keyword + PerformSpecialAction) ---");
        engine.RunTurn();

        Console.WriteLine("--- Command queue (Command Pattern stretch goal) ---");
        engine.RunTurnWithCommands();

        Console.WriteLine("\nPress any key to continue to the Character Manager...");
        Console.ReadKey();
        Console.Clear();
    }

    /// <summary>
    /// Switches the active file format using FileHandlerFactory (DIP).
    /// Program.cs never references CsvFileHandler or JsonFileHandler directly.
    /// </summary>
    private static void SwitchFileFormat()
    {
        string selectedFormat = _menu.GetFileFormatChoice();

        if (string.IsNullOrEmpty(selectedFormat))
        {
            _menu.DisplayFormatChangeCancelled();
            return;
        }

        // DIP: use factory - Program.cs depends on the abstraction (IFileHandler),
        // not the concrete implementations.
        _fileHandler = FileHandlerFactory.Create(selectedFormat);
        _currentFormat = selectedFormat.ToUpper();
        _characterUI = new CharacterUI(_fileHandler);

        _menu.DisplayFormatChanged(_currentFormat);
    }
}
