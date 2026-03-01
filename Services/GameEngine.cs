using W5SolidLsp.Interfaces;
using W5SolidLsp.Services.Commands;

namespace W5SolidLsp.Services;

/// <summary>
/// Runs the game loop and processes entities.
/// Depends only on IEntity (DIP) — never on concrete classes like Ghost or Goblin.
///
/// Uses the 'is' keyword to safely check for optional capabilities before calling them.
/// This satisfies LSP: we never assume an IEntity can fly, shoot, swim, or defend.
///
/// Also demonstrates the Command Pattern (stretch goal) by building a command queue
/// from the entity list and executing each command in sequence.
/// </summary>
public class GameEngine
{
    private readonly List<IEntity> _entities;

    /// <summary>
    /// Initializes a new GameEngine with the given list of entities.
    /// </summary>
    /// <param name="entities">The entities to process each turn.</param>
    public GameEngine(List<IEntity> entities)
    {
        _entities = entities;
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
