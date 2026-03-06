using W6DependencyInversion.Interfaces;
using W6DependencyInversion.Models.Characters;
using W6DependencyInversion.Services.Commands;

namespace W6DependencyInversion.Services;

/// <summary>
/// Runs the game loop and processes entities.
/// Depends only on IEntity (DIP) - never on concrete classes like Ghost or Goblin.
///
/// W6 addition: also calls PerformSpecialAction() via CharacterBase when the entity
/// is a character, demonstrating DIP - GameEngine depends on the abstraction.
/// </summary>
public class GameEngine
{
    private readonly List<IEntity> _entities;

    /// <summary>
    /// Initializes a new GameEngine with the given list of entities.
    /// DIP: accepts the abstraction IEntity, not concrete types.
    /// </summary>
    public GameEngine(List<IEntity> entities)
    {
        _entities = entities;
    }

    /// <summary>
    /// Processes all entities for one game turn.
    /// Every entity attacks; optional abilities are checked with 'is' before use.
    /// If the entity is a CharacterBase, PerformSpecialAction() is also called.
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
    public void ProcessEntity(IEntity entity)
    {
        // All entities can attack - safe to call directly
        entity.Attack();

        // Only call Defend() if the entity implements IDefendable - LSP safe
        if (entity is IDefendable defendingEntity)
        {
            defendingEntity.Defend();
        }

        // Only call Fly() if the entity implements IFlyable - LSP safe
        if (entity is IFlyable flyingEntity)
        {
            flyingEntity.Fly();
        }

        // Only call Shoot() if the entity implements IShootable - LSP safe
        if (entity is IShootable shootingEntity)
        {
            shootingEntity.Shoot();
        }

        // Only call Swim() if the entity implements ISwimmable - LSP safe
        if (entity is ISwimmable swimmingEntity)
        {
            swimmingEntity.Swim();
        }

        // W6: call PerformSpecialAction() via the CharacterBase abstraction (DIP)
        if (entity is CharacterBase character)
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
            commands.Add(new AttackCommand(entity));

            if (entity is IDefendable defender)
                commands.Add(new DefendCommand(defender));

            if (entity is IFlyable flier)
                commands.Add(new FlyCommand(flier));

            if (entity is IShootable shooter)
                commands.Add(new ShootCommand(shooter));

            if (entity is ISwimmable swimmer)
                commands.Add(new SwimCommand(swimmer));
        }

        foreach (ICommand command in commands)
        {
            command.Execute();
        }
    }
}
