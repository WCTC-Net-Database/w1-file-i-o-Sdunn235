using ConsoleRpg.Decorators;
using ConsoleRpg.Helpers;
using ConsoleRpg.Interfaces;
using ConsoleRpg.Services;
using ConsoleRpg.UI;
using ConsoleRpgEntities.Data;
using ConsoleRpgEntities.Models;

namespace ConsoleRpg;

/// <summary>
/// Composition root — the only place in the solution that calls 'new' on concrete types.
/// All services are wired here and injected as abstractions into GameEngine and other consumers.
///
/// DIP in action: Nothing downstream of this class knows about GameContext, PlayerDao,
/// BattleService, or any other concretion. Every consumer depends on an interface.
///
/// Program.cs calls Startup.Initialize() then hands off to GameEngine and other services.
/// </summary>
public static class Startup
{
    private static IFileHandler _fileHandler = null!;
    private static MenuService _menu = null!;
    private static string _currentFormat = "CSV";

    public static IGameUi GameUi { get; private set; } = null!;
    public static GameEngine GameEngine { get; private set; } = null!;
    public static GameEngineDemo GameEngineDemo { get; private set; } = null!;
    public static CharacterUI CharacterUi { get; private set; } = null!;

    /// <summary>
    /// Wires all dependencies and initializes every service.
    /// Call once at application startup — before anything else runs.
    /// </summary>
    public static void Initialize()
    {
        // Configuration — file paths come from appsettings.json, not hardcoded strings
        var config = ConfigurationHelper.LoadDataFileConfig();

        // Data layer — GameContext is the only class that touches JSON files directly
        var context = new GameContext(config.Players, config.Monsters, config.Items);
        context.Read();

        // DAO layer — each DAO depends on IContext, never on GameContext
        IEntityDao<Player> playerDao = new PlayerDao(context);

        // Service layer — wrap PlayerService in AutoSave decorator
        // GameEngine sees only IPlayerService — the decorator is invisible to it
        IPlayerService playerService = new PlayerService(context, playerDao);
        playerService = new AutoSavePlayerServiceDecorator(playerService, context);

        IBattleService battleService = new BattleService();
        GameUi = new ConsoleGameUi();

        // W7 GameEngine — all dependencies are abstractions
        GameEngine = new GameEngine(context, playerService, battleService, GameUi);

        // W6 Demo and Character Manager — wired separately, accessible via menu
        _fileHandler = new CsvFileHandler("Input/input.csv");
        GameEngineDemo = new GameEngineDemo(_fileHandler);
        CharacterUi = new CharacterUI(_fileHandler);
        _menu = new MenuService();
    }

    /// <summary>
    /// Runs the W6 character manager sub-menu loop.
    /// Extracted here (SRP) so Program.cs stays lean.
    /// </summary>
    public static void RunCharacterManager()
    {
        bool running = true;
        while (running)
        {
            Console.WriteLine($"\n[Format: {_currentFormat}]");
            _menu.DisplayMainMenu();
            string choice = _menu.GetMenuChoice();

            switch (choice)
            {
                case "1": CharacterUi.DisplayAllCharacters(); break;
                case "2": CharacterUi.FindCharacter(); break;
                case "3": CharacterUi.AddCharacter(); break;
                case "4": CharacterUi.LevelUpCharacter(); break;
                case "5": SwitchFormat(); break;
                case "0": running = false; break;
                default: Console.WriteLine("Invalid choice."); break;
            }

            if (running) _menu.PauseAndClear();
        }
    }

    private static void SwitchFormat()
    {
        string selectedFormat = _menu.GetFileFormatChoice();

        if (string.IsNullOrEmpty(selectedFormat))
        {
            _menu.DisplayFormatChangeCancelled();
            return;
        }

        _fileHandler = selectedFormat.ToLower() switch
        {
            "csv"  => new CsvFileHandler("Input/input.csv"),
            "json" => new JsonFileHandler("Input/input.json"),
            _      => _fileHandler
        };

        _currentFormat = selectedFormat.ToUpper();
        CharacterUi = new CharacterUI(_fileHandler);
        _menu.DisplayFormatChanged(_currentFormat);
    }
}
