using ConsoleRpgEntities.Models;

namespace ConsoleRpgEntities.Data;

/// <summary>
/// The DIP anchor for all data access in ConsoleRpg.
/// ConsoleRpg depends on this abstraction — never on any concrete context directly.
///
/// Week 4:  IFileHandler worked with one entity type (Characters).
/// Week 7:  IContext works with multiple entity types via in-memory lists.
/// Week 9:  IContext evolves — both FileContext (JSON) and GameContext (EF Core DbContext)
///          satisfy this contract. Business logic never knows which back-end is in use.
///
/// Collections are IEnumerable — the common ground between List&lt;T&gt; and DbSet&lt;T&gt;.
/// Mutations go through AddEntity/RemoveEntity so each context can handle them its own way.
/// </summary>
public interface IContext
{
    // Existing entity collections
    IEnumerable<Player> Players { get; }
    IEnumerable<MonsterBase> Monsters { get; }
    IEnumerable<Item> Items { get; }

    // W9: New entity collections for EF Core
    IEnumerable<Room> Rooms { get; }
    IEnumerable<Character> Characters { get; }

    /// <summary>Adds an entity to the context. FileContext dispatches by type; DbContext tracks it.</summary>
    void AddEntity<T>(T entity) where T : class;

    /// <summary>Removes an entity from the context.</summary>
    void RemoveEntity<T>(T entity) where T : class;

    /// <summary>Persists all staged changes to the data source (JSON file or database).</summary>
    void SaveChanges();
}
