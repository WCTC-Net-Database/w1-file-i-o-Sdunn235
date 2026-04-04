using ConsoleRpg.Interfaces;
using ConsoleRpg.Services.Commands;
using ConsoleRpg.UI;
using ConsoleRpgEntities.Data;
using ConsoleRpgEntities.Models;
// Alias to resolve W6 Character (abstract class) vs W9 Character (EF entity) namespace collision
using EfCharacter = ConsoleRpgEntities.Models.Character;

namespace ConsoleRpg.Services;

/// <summary>
/// Runs the game loop and processes entities.
///
/// Three constructors support three contexts:
///   W6 constructor: (IFileHandler, List&lt;IEntity&gt;) — powers RunTurn() and the entity demo.
///   W7 constructor: (IContext, IPlayerService, IBattleService, IGameUi) — powers RunCombat().
///   W9 constructor: Extends W7 with a second IContext for EF Core database operations.
///
/// DIP: All constructors depend only on abstractions — no concrete types anywhere.
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

    // W9 field — EF Core database context (accessed through IContext abstraction)
    private readonly IContext? _dbContext;

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
    /// W7 Constructor: used when only file-based context is needed (no EF Core).
    /// All four parameters are abstractions — Startup decides which concretions to inject.
    /// </summary>
    public GameEngine(IContext context, IPlayerService playerService,
                      IBattleService battleService, IGameUi gameUi)
        : this(context, playerService, battleService, gameUi, dbContext: null)
    {
    }

    /// <summary>
    /// W9 Constructor: full game engine with both file-based and EF Core contexts.
    /// fileContext powers W7 combat features (JSON-backed).
    /// dbContext powers W9 CRUD features (SQL Server-backed).
    /// Both are IContext — business logic never knows which back-end is in use.
    /// </summary>
    public GameEngine(IContext fileContext, IPlayerService playerService,
                      IBattleService battleService, IGameUi gameUi,
                      IContext? dbContext)
    {
        _context = fileContext;
        _playerService = playerService;
        _battleService = battleService;
        _gameUi = gameUi;
        _dbContext = dbContext;
        _entities = new List<IEntity>();
        _fileHandler = null!;
    }

    /// <summary>
    /// Uses the injected IFileHandler to read and display all characters from the data source.
    /// DIP demo: GameEngine calls _fileHandler.ReadAll() on an abstraction �
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
        // All entities can attack � safe to call directly
        entity.Attack();

        // Only call Defend() if the entity implements IDefendable � LSP safe
        if (entity is IDefendable defendingEntity)
        {
            defendingEntity.Defend();
        }

        // Only call Fly() if the entity implements IFlyable � LSP safe
        if (entity is IFlyable flyingEntity)
        {
            flyingEntity.Fly();
        }

        // Only call Shoot() if the entity implements IShootable � LSP safe
        if (entity is IShootable shootingEntity)
        {
            shootingEntity.Shoot();
        }

        // Only call Swim() if the entity implements ISwimmable � LSP safe
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
    // W7 Methods � use IContext, IPlayerService, IBattleService, IGameUi
    // -------------------------------------------------------------------------

    /// <summary>
    /// Runs one round of combat between the player and the first available monster.
    /// BattleService handles all LINQ damage calculations � GameEngine only orchestrates.
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
        if (!monsters.Any()) { _gameUi.DisplayMessage("No monsters remain � you've won!"); return; }

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
            _context.RemoveEntity(monster);
        }

        if (player.Hp <= 0)
        {
            _gameUi.DisplayMessage($"{player.Name} has fallen in battle...");
            if (_gameUi.AskResetBattle())
                ResetBattle();
        }
        else
        {
            // AutoSaveDecorator handles SaveChanges � no explicit save call needed here
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
        // Read() is a FileContext-specific operation (reloads from JSON files).
        // Safe cast: ResetBattle only applies to the file-backed context.
        if (_context is FileContext fileContext)
            fileContext.Read();

        // Heal player back to full
        var player = _playerService.GetAllPlayers().FirstOrDefault();
        if (player != null)
        {
            player.Hp = player.MaxHp;
            _playerService.UpdatePlayer(player);
        }

        _gameUi.DisplayMessage("Battle reset � all monsters restored and player healed to full HP.");
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

    // -------------------------------------------------------------------------
    // W9 Methods — use _dbContext (EF Core IContext) for database CRUD
    // -------------------------------------------------------------------------

    /// <summary>
    /// Displays all characters in the database with their room assignments.
    /// Lazy loading proxies handle Room navigation — no explicit Include() needed.
    /// LINQ: ToList() materializes the query; Room loads automatically on first access.
    /// </summary>
    public void DisplayCharacters()
    {
        if (_dbContext == null) { Console.WriteLine("EF Core context not initialized."); return; }

        var characters = _dbContext.Characters.OfType<EfCharacter>().ToList();

        if (!characters.Any())
        {
            Console.WriteLine("\nNo characters found in the database.");
            return;
        }

        Console.WriteLine("\n=== Characters (EF Core — SQL Server) ===\n");
        foreach (var c in characters)
        {
            Console.WriteLine($"  [{c.Id}] {c.Name} (Level {c.Level}) — Room: {c.Room?.Name ?? "None"}");
        }
    }

    /// <summary>
    /// Searches for a character by name using LINQ.
    /// Uses FirstOrDefault with a Contains predicate for partial matching.
    /// </summary>
    public void FindCharacter()
    {
        if (_dbContext == null) { Console.WriteLine("EF Core context not initialized."); return; }

        Console.Write("Enter character name to search: ");
        var name = Console.ReadLine() ?? string.Empty;

        var character = _dbContext.Characters
            .OfType<EfCharacter>()
            .FirstOrDefault(c => c.Name.Contains(name));

        if (character != null)
        {
            Console.WriteLine($"\nFound: {character.Name} (Level {character.Level})");
        }
        else
        {
            Console.WriteLine("\nCharacter not found.");
        }
    }

    /// <summary>
    /// Creates a new character and associates it with an existing room.
    /// Validates that the room exists before creating the character.
    /// Uses AddEntity + SaveChanges to persist to SQL Server.
    /// </summary>
    public void AddCharacter()
    {
        if (_dbContext == null) { Console.WriteLine("EF Core context not initialized."); return; }

        Console.Write("Enter character name: ");
        var name = Console.ReadLine() ?? string.Empty;

        Console.Write("Enter character level: ");
        if (!int.TryParse(Console.ReadLine(), out var level))
        {
            Console.WriteLine("Invalid level.");
            return;
        }

        Console.Write("Enter room ID for the character: ");
        if (!int.TryParse(Console.ReadLine(), out var roomId))
        {
            Console.WriteLine("Invalid room ID.");
            return;
        }

        // Validate the room exists before creating the character
        var room = _dbContext.Rooms
            .OfType<Room>()
            .FirstOrDefault(r => r.Id == roomId);

        if (room == null)
        {
            Console.WriteLine("Room not found!");
            return;
        }

        var character = new EfCharacter
        {
            Name = name,
            Level = level,
            RoomId = roomId
        };

        _dbContext.AddEntity(character);
        _dbContext.SaveChanges();

        Console.WriteLine($"\nCharacter '{name}' added to {room.Name}.");
    }

    /// <summary>
    /// Creates a new room in the database.
    /// Uses AddEntity + SaveChanges to persist to SQL Server.
    /// </summary>
    public void AddRoom()
    {
        if (_dbContext == null) { Console.WriteLine("EF Core context not initialized."); return; }

        Console.Write("Enter room name: ");
        var name = Console.ReadLine() ?? string.Empty;

        Console.Write("Enter room description: ");
        var description = Console.ReadLine() ?? string.Empty;

        var room = new Room
        {
            Name = name,
            Description = description
        };

        _dbContext.AddEntity(room);
        _dbContext.SaveChanges();

        Console.WriteLine($"\nRoom '{name}' added to the game.");
    }

    /// <summary>
    /// Displays all rooms in the database with their ID, name, and description.
    /// Useful as a reference before adding a character (which requires a room ID).
    /// </summary>
    public void DisplayRooms()
    {
        if (_dbContext == null) { Console.WriteLine("EF Core context not initialized."); return; }

        var rooms = _dbContext.Rooms.ToList();

        if (!rooms.Any())
        {
            Console.WriteLine("\nNo rooms found in the database.");
            return;
        }

        Console.WriteLine("\n=== Rooms (EF Core — SQL Server) ===\n");
        foreach (var r in rooms)
        {
            Console.WriteLine($"  [{r.Id}] {r.Name} — {r.Description}");
        }
    }

    /// <summary>
    /// Stretch Goal: Finds a character by exact name and increments their level.
    /// Demonstrates EF Core change tracking — modifying the entity in memory
    /// and calling SaveChanges() generates an UPDATE SQL statement automatically.
    /// </summary>
    public void LevelUpCharacter()
    {
        if (_dbContext == null) { Console.WriteLine("EF Core context not initialized."); return; }

        Console.Write("Enter character name: ");
        var name = Console.ReadLine() ?? string.Empty;

        var character = _dbContext.Characters
            .OfType<EfCharacter>()
            .FirstOrDefault(c => c.Name == name);

        if (character != null)
        {
            character.Level++;
            _dbContext.SaveChanges();
            Console.WriteLine($"\n{character.Name} is now level {character.Level}!");
        }
        else
        {
            Console.WriteLine("\nCharacter not found.");
        }
    }
}
