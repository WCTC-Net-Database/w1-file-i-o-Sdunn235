using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ConsoleRpgEntities.Models;

namespace ConsoleRpgEntities.Data;

/// <summary>
/// EF Core database context — manages the SQL Server connection and entity persistence.
/// Implements both DbContext (EF Core machinery) and IContext (our DIP abstraction).
///
/// Week 9: This is the real-database counterpart to FileContext (JSON-backed).
///         Both satisfy IContext — business logic never knows which back-end is in use.
///
/// DbSet properties give EF Core the table mappings.
/// IContext properties expose them as IEnumerable for downstream consumers.
/// AddEntity/RemoveEntity delegate to EF Core's generic Set&lt;T&gt;() methods.
/// SaveChanges() is inherited from DbContext and satisfies IContext automatically.
/// </summary>
public class GameContext : DbContext, IContext
{
    // EF Core DbSets — these map to SQL Server tables
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Character> Characters { get; set; }

    // IContext — existing entity collections (not DB-backed yet — returns empty)
    // These can be migrated to DbSets in future weeks as needed.
    IEnumerable<Player> IContext.Players => Enumerable.Empty<Player>();
    IEnumerable<MonsterBase> IContext.Monsters => Enumerable.Empty<MonsterBase>();
    IEnumerable<Item> IContext.Items => Enumerable.Empty<Item>();

    // IContext — W9 entity collections backed by DbSets
    IEnumerable<Room> IContext.Rooms => Rooms;
    IEnumerable<Character> IContext.Characters => Characters;

    /// <summary>
    /// Adds an entity to the appropriate DbSet via EF Core's generic Set resolution.
    /// Same contract as FileContext.AddEntity — different implementation, same abstraction.
    /// </summary>
    public void AddEntity<T>(T entity) where T : class
    {
        Set<T>().Add(entity);
    }

    /// <summary>
    /// Removes an entity from the appropriate DbSet via EF Core's generic Set resolution.
    /// </summary>
    public void RemoveEntity<T>(T entity) where T : class
    {
        Set<T>().Remove(entity);
    }

    /// <summary>
    /// Explicit IContext.SaveChanges implementation.
    /// DbContext.SaveChanges() returns int (rows affected), but IContext expects void.
    /// This bridges the two contracts.
    /// </summary>
    void IContext.SaveChanges()
    {
        base.SaveChanges();
    }

    /// <summary>
    /// Configures the SQL Server connection and enables lazy loading proxies.
    /// Connects to the WCTC school SQL Server at bitsql.wctc.edu.
    ///
    /// Connection string is loaded from appsettings.json, then overridden by
    /// appsettings.Development.json if present. The Development file holds the
    /// real password and is gitignored — appsettings.json contains only a
    /// placeholder that is safe to commit.
    ///
    /// Lazy loading proxies allow navigation properties (marked virtual) to load
    /// automatically on first access — no explicit Include() calls needed.
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
            .Build();

        var connectionString = configuration.GetConnectionString("GameDb")
            ?? throw new InvalidOperationException(
                "Connection string 'GameDb' not found. Ensure appsettings.json is present " +
                "and (for real credentials) appsettings.Development.json exists in the output directory.");

        optionsBuilder
            .UseLazyLoadingProxies()
            .UseSqlServer(connectionString);
    }
}
