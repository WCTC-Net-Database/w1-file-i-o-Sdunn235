using ConsoleRpg.Interfaces;
using ConsoleRpg.Services.Commands;
using ConsoleRpg.UI;
using ConsoleRpgEntities.Data;
using ConsoleRpgEntities.Models;

namespace ConsoleRpg.Services;

/// <summary>
/// Runs the game loop and processes entities.
///
/// Two constructors support two contexts:
///   W6 constructor: (IFileHandler, List&lt;IEntity&gt;) — powers RunTurn() and the entity demo.
///   W7 constructor: (IContext, IPlayerService, IBattleService, IGameUi) — powers RunCombat().
///
/// DIP: Both constructors depend only on abstractions — no concrete types anywhere.
/// </summary>
public class GameEngine
{
    // W6 fields
    private readonly List<IEntity> _entities;
    private readonly IFileHandler _fileHandler;

    // W7 fields
    private readonly IContext? _context;
    private readonly IPlayerService? _playerService;
    private readonly IBattleService? _battleService;
    private readonly IGameUi? _gameUi;

    /// <summary>
    /// W6 Constructor: used by GameEngineDemo for the entity-turn demonstration.
    /// DIP in action: GameEngine never creates its own CsvFileHandler or concrete entity.
    /// </summary>
    public GameEngine(IFileHandler fileHandler, List<IEntity> entities)
    {
        _fileHandler = fileHandler;
        _entities = entities;
    }

    /// <summary>
    /// W7 Constructor: used by Startup for the full game loop.
    /// All four parameters are abstractions — Startup decides which concretions to inject.
    /// </summary>
    public GameEngine(IContext context, IPlayerService playerService,
                      IBattleService battleService, IGameUi gameUi)
    {
        _context = context;
        _playerService = playerService;
        _battleService = battleService;
        _gameUi = gameUi;
        _entities = new List<IEntity>();
        _fileHandler = null!;
    }

    /// <summary>
    /// Uses the injected IFileHandler to read and display all characters from the data source.
    /// DIP demo: GameEngine calls _fileHandler.ReadAll() on an abstraction —
    /// it has no idea whether the source is CSV, JSON, or anything else.
    /// </summary>
    public void DisplayLoadedCharacters()
    {
        Console.WriteLine("\n=== Characters Loaded via IFileHandler (DIP) ===\n");

        var characters = _fileHandler.ReadAll();

        if (characters.Count == 0)
        {
            Console.WriteLine("No characters found in the data source.");
            return;
        }

        foreach (var character in characters)
        {
            Console.WriteLine($"  {character}");
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Processes all entities for one game turn.
    /// Every entity attacks; optional abilities are checked with 'is' before use.
    /// </summary>
    public void RunTurn()
    {
        Console.WriteLine("\n=== Game Engine: Processing Entities ===\n");

        foreach (IEntity entity in _entities)
        {
            Console.WriteLine($"--- {entity.Name} ---");
            ProcessEntity(entity);
            Console.WriteLine();
        }
    }

    /// <summary>
    /// Processes a single entity, calling all abilities it supports.
    /// Uses the 'is' keyword pattern to check each optional interface.
    /// </summary>
    /// <param name="entity">The entity to process.</param>
    public void ProcessEntity(IEntity entity)
    {
        // All entities can attack — safe to call directly
        entity.Attack();

        // Only call Defend() if the entity implements IDefendable — LSP safe
        if (entity is IDefendable defendingEntity)
        {
            defendingEntity.Defend();
        }

        // Only call Fly() if the entity implements IFlyable — LSP safe
        if (entity is IFlyable flyingEntity)
        {
            flyingEntity.Fly();
        }

        // Only call Shoot() if the entity implements IShootable — LSP safe
        if (entity is IShootable shootingEntity)
        {
            shootingEntity.Shoot();
        }

        // Only call Swim() if the entity implements ISwimmable — LSP safe
        if (entity is ISwimmable swimmingEntity)
        {
            swimmingEntity.Swim();
        }

        // W6 DIP: Call PerformSpecialAction if the entity is a CharacterBase (abstraction check).
        // GameEngine depends on the abstraction, not on any concrete type.
        if (entity is ConsoleRpg.Models.Characters.CharacterBase character)
        {
            character.PerformSpecialAction();
        }
    }

    /// <summary>
    /// Stretch Goal: Runs the same turn using the Command Pattern.
    /// Builds a queue of ICommand objects from the entity list and executes each.
    /// </summary>
    public void RunTurnWithCommands()
    {
        Console.WriteLine("\n=== Game Engine: Command Queue ===\n");

        var commands = new List<ICommand>();

        foreach (IEntity entity in _entities)
        {
            // Every entity gets an attack command
            commands.Add(new AttackCommand(entity));

            // Optional commands added only when the entity supports the interface
            if (entity is IDefendable defender)
                commands.Add(new DefendCommand(defender));

            if (entity is IFlyable flier)
                commands.Add(new FlyCommand(flier));

            if (entity is IShootable shooter)
                commands.Add(new ShootCommand(shooter));

            if (entity is ISwimmable swimmer)
                commands.Add(new SwimCommand(swimmer));
        }

        // Execute all queued commands
        foreach (ICommand command in commands)
        {
            command.Execute();
        }
    }

    // -------------------------------------------------------------------------
    // W7 Methods — use IContext, IPlayerService, IBattleService, IGameUi
    // -------------------------------------------------------------------------

    /// <summary>
    /// Runs one round of combat between the player and the first available monster.
    /// BattleService handles all LINQ damage calculations — GameEngine only orchestrates.
    /// AutoSaveDecorator on IPlayerService persists changes automatically after update.
    /// </summary>
    public void RunCombat()
    {
        if (_context == null || _playerService == null || _battleService == null || _gameUi == null)
        {
            Console.WriteLine("GameEngine not initialized for W7 combat.");
            return;
        }

        var player = _playerService.GetAllPlayers().FirstOrDefault();
        if (player == null) { _gameUi.DisplayMessage("No player found. Check players.json."); return; }

        var monsters = _context.Monsters.Where(m => m.Hp > 0).ToList();
        if (!monsters.Any()) { _gameUi.DisplayMessage("No monsters remain — you've won!"); return; }

        _gameUi.DisplayPlayer(player);
        _gameUi.DisplayMonsters(monsters);

        var monster = monsters.First();
        _gameUi.DisplayMessage($"A {monster.Name} steps forward to fight!");

        string result = _battleService.ResolveCombat(player, monster);
        _gameUi.DisplayCombatResult(result);

        if (monster.Hp <= 0)
        {
            monster.PerformSpecialAction();
            _gameUi.DisplayMessage($"{monster.Name} has been defeated!");
            _context.Monsters.Remove(monster);
        }

        if (player.Hp <= 0)
        {
            _gameUi.DisplayMessage($"{player.Name} has fallen in battle...");
            if (_gameUi.AskResetBattle())
                ResetBattle();
        }
        else
        {
            // AutoSaveDecorator handles SaveChanges — no explicit save call needed here
            _playerService.UpdatePlayer(player);
        }
    }

    /// <summary>
    /// Resets the battle to its original state.
    /// Re-reads all data from JSON (restores monsters to full HP) and heals the player to MaxHp.
    /// Callable from the menu (player choice) or automatically triggered on player death.
    /// </summary>
    public void ResetBattle()
    {
        if (_context == null || _playerService == null || _gameUi == null) return;

        // Re-read all JSON data — monsters restore to their original HP values
        _context.Read();

        // Heal player back to full
        var player = _playerService.GetAllPlayers().FirstOrDefault();
        if (player != null)
        {
            player.Hp = player.MaxHp;
            _playerService.UpdatePlayer(player);
        }

        _gameUi.DisplayMessage("Battle reset — all monsters restored and player healed to full HP.");
    }

    /// <summary>
    /// Displays the current player's stats and equipped items via IGameUi.
    /// </summary>
    public void ViewPlayer()
    {
        if (_playerService == null || _gameUi == null) return;

        var player = _playerService.GetAllPlayers().FirstOrDefault();
        if (player == null) { _gameUi.DisplayMessage("No player found."); return; }

        _gameUi.DisplayPlayer(player);
    }
}
