using W6SolidDip.Interfaces;
using W6SolidDip.Services.Commands;

namespace W6SolidDip.Services;

/// <summary>
/// Runs the game loop and processes entities.
///
/// DIP: GameEngine depends on abstractions (IEntity, IFileHandler), never on
/// concrete classes like Ghost, Goblin, CsvFileHandler, or JsonFileHandler.
/// Dependencies are injected through the constructor — the caller decides
/// which implementations to provide.
///
/// Uses the 'is' keyword to safely check for optional capabilities (LSP/ISP).
/// Also demonstrates the Command Pattern via RunTurnWithCommands.
/// </summary>
public class GameEngine
{
    private readonly List<IEntity> _entities;
    private readonly IFileHandler _fileHandler;

    /// <summary>
    /// Initializes a new GameEngine with injected dependencies.
    /// DIP in action: GameEngine never creates its own CsvFileHandler or concrete entity.
    /// </summary>
    /// <param name="fileHandler">Abstraction for file I/O — injected, not created here.</param>
    /// <param name="entities">The entities to process each turn.</param>
    public GameEngine(IFileHandler fileHandler, List<IEntity> entities)
    {
        _fileHandler = fileHandler;
        _entities = entities;
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
        if (entity is W6SolidDip.Models.Characters.CharacterBase character)
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
}
