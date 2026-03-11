using ConsoleRpgEntities.Models;

namespace ConsoleRpgEntities.Data;

/// <summary>
/// The DIP anchor for all data access in ConsoleRpg.
/// ConsoleRpg depends on this abstraction — never on GameContext directly.
///
/// Week 4: IFileHandler worked with one entity type (Characters).
/// Week 7: IContext works with multiple entity types (Players, Monsters, Items).
/// Week 9: IContext becomes DbContext with real database support.
///
/// SaveChanges() mimics database behavior: make changes in memory, then
/// commit them all at once — rather than writing to disk on every operation.
/// </summary>
public interface IContext
{
    List<Player> Players { get; }
    List<MonsterBase> Monsters { get; }
    List<Item> Items { get; }

    /// <summary>Loads all entities from their data source into memory.</summary>
    void Read();

    /// <summary>Stages an entity to be added to the appropriate in-memory list.</summary>
    void Write(object entity);

    /// <summary>Persists all staged in-memory changes back to the data source.</summary>
    void SaveChanges();
}
