using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ConsoleRpgEntities.Models;
using ConsoleRpgEntities.Models.Abilities;

namespace ConsoleRpgEntities.Data;

/// <summary>
/// EF Core database context — manages the SQL Server connection and entity persistence.
/// Implements both DbContext (EF Core machinery) and IContext (our DIP abstraction).
///
/// Week 10: OnModelCreating configures TPH for Character and Ability hierarchies,
///          plus the many-to-many Character ↔ Ability relationship via CharacterAbilities.
/// </summary>
public class GameContext : DbContext, IContext
{
    // EF Core DbSets — these map to SQL Server tables
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Character> Characters { get; set; }
    public DbSet<Ability> Abilities { get; set; }

    // IContext — existing entity collections (not DB-backed yet — returns empty)
    // These can be migrated to DbSets in future weeks as needed.
    IEnumerable<Player> IContext.Players => Characters.OfType<Player>();
    IEnumerable<MonsterBase> IContext.Monsters => Enumerable.Empty<MonsterBase>();
    IEnumerable<Item> IContext.Items => Enumerable.Empty<Item>();

    // IContext — W9 entity collections backed by DbSets
    IEnumerable<Room> IContext.Rooms => Rooms;
    IEnumerable<Character> IContext.Characters => Characters;

    /// <summary>
    /// Adds an entity to the appropriate DbSet via EF Core's generic Set resolution.
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
    /// </summary>
    void IContext.SaveChanges()
    {
        base.SaveChanges();
    }

    /// <summary>
    /// Configures the SQL Server connection and enables lazy loading proxies.
    /// Connection string loaded from appsettings.json, overridden by appsettings.Development.json.
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

    /// <summary>
    /// Configures TPH inheritance for Character and Ability hierarchies,
    /// plus the many-to-many relationship between them via a CharacterAbilities join table.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // TPH for Characters — Discriminator column distinguishes Player vs Goblin
        modelBuilder.Entity<Character>()
            .HasDiscriminator<string>("Discriminator")
            .HasValue<Player>("Player")
            .HasValue<Goblin>("Goblin");

        // TPH for Abilities — AbilityType column distinguishes PlayerAbility vs GoblinAbility
        modelBuilder.Entity<Ability>()
            .HasDiscriminator<string>("AbilityType")
            .HasValue<PlayerAbility>("PlayerAbility")
            .HasValue<GoblinAbility>("GoblinAbility");

        // Many-to-many: Character ↔ Ability via explicit join table
        modelBuilder.Entity<Character>()
            .HasMany(c => c.Abilities)
            .WithMany(a => a.Characters)
            .UsingEntity(j => j.ToTable("CharacterAbilities"));

        base.OnModelCreating(modelBuilder);
    }
}
