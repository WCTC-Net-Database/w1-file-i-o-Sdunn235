namespace ConsoleRpgEntities.Data;

/// <summary>
/// Generic DAO interface defining standard CRUD operations for any entity type.
///
/// ISP: Each entity gets its own DAO (PlayerDao, MonsterDao) implementing this interface,
/// rather than one bloated class handling all entity types.
///
/// DIP: Services in ConsoleRpg depend on IEntityDao&lt;T&gt; (abstraction),
/// not on PlayerDao or MonsterDao (concretions).
/// </summary>
public interface IEntityDao<T>
{
    /// <summary>Returns the entity with the matching Id, or null if not found.</summary>
    T? GetById(int id);

    /// <summary>Returns all entities of this type.</summary>
    IEnumerable<T> GetAll();

    /// <summary>Adds a new entity to the in-memory list.</summary>
    void Add(T entity);

    /// <summary>Updates an existing entity's properties in the in-memory list.</summary>
    void Update(T entity);

    /// <summary>Removes the entity with the matching Id from the in-memory list.</summary>
    void Delete(int id);
}
