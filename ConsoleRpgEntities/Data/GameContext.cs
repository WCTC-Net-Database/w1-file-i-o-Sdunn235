using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ConsoleRpgEntities.Models;
using ConsoleRpgEntities.Models.Abilities;
using ConsoleRpgEntities.Models.Items;
using ConsoleRpgEntities.Models.Magic;
using ConsoleRpgEntities.Models.Races;
using ConsoleRpgEntities.Models.Skills;

namespace ConsoleRpgEntities.Data;

public class GameContext : DbContext, IContext
{
    // DbSets
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Door> Doors { get; set; }
    public DbSet<Character> Characters { get; set; }
    public DbSet<Race> Races { get; set; }
    public DbSet<Stats> Stats { get; set; }
    public DbSet<Resources> Resources { get; set; }
    public DbSet<Ability> Abilities { get; set; }
    public DbSet<Magic> Magics { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<EquipmentSlot> EquipmentSlots { get; set; }
    public DbSet<Skill> Skills { get; set; }
    public DbSet<CharacterSkill> CharacterSkills { get; set; }

    // IContext explicit implementations
    IEnumerable<Room> IContext.Rooms => Rooms;
    IEnumerable<Door> IContext.Doors => Doors;
    IEnumerable<Character> IContext.Characters => Characters;
    IEnumerable<Race> IContext.Races => Races;
    IEnumerable<Stats> IContext.Stats => Stats;
    IEnumerable<Resources> IContext.Resources => Resources;
    IEnumerable<Ability> IContext.Abilities => Abilities;
    IEnumerable<Magic> IContext.Magics => Magics;
    IEnumerable<Item> IContext.Items => Items;
    IEnumerable<EquipmentSlot> IContext.EquipmentSlots => EquipmentSlots;
    IEnumerable<Skill> IContext.Skills => Skills;

    public void AddEntity<T>(T entity) where T : class
    {
        Set<T>().Add(entity);
    }

    public void RemoveEntity<T>(T entity) where T : class
    {
        Set<T>().Remove(entity);
    }

    void IContext.SaveChanges()
    {
        base.SaveChanges();
    }

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // --- Character TPH ---
        modelBuilder.Entity<Character>()
            .HasDiscriminator<string>("CharacterType")
            .HasValue<Player>("Player")
            .HasValue<Npc>("NPC")
            .HasValue<Animal>("Animal");

        // --- Race TPH ---
        modelBuilder.Entity<Race>()
            .HasDiscriminator<string>("RaceType")
            .HasValue<PlayableRace>("Playable")
            .HasValue<MonsterRace>("Monster")
            .HasValue<AnimalRace>("Animal");

        // --- Item TPH ---
        modelBuilder.Entity<Item>()
            .HasDiscriminator<string>("ItemType")
            .HasValue<Weapon>("Weapon")
            .HasValue<Armor>("Armor")
            .HasValue<Consumable>("Consumable");

        // --- Character relationships ---

        // Character → Stats (one-to-one)
        modelBuilder.Entity<Character>()
            .HasOne(c => c.Stats)
            .WithOne(s => s.Character)
            .HasForeignKey<Stats>(s => s.CharacterId);

        // Character → Resources (one-to-one)
        modelBuilder.Entity<Character>()
            .HasOne(c => c.Resources)
            .WithOne(r => r.Character)
            .HasForeignKey<Resources>(r => r.CharacterId);

        // Character → Race (many-to-one, nullable)
        modelBuilder.Entity<Character>()
            .HasOne(c => c.Race)
            .WithMany(r => r.Characters)
            .HasForeignKey(c => c.RaceId)
            .OnDelete(DeleteBehavior.SetNull);

        // Character → Room (many-to-one, nullable)
        modelBuilder.Entity<Character>()
            .HasOne(c => c.Room)
            .WithMany(r => r.Characters)
            .HasForeignKey(c => c.RoomId)
            .OnDelete(DeleteBehavior.SetNull);

        // Character ↔ Ability (many-to-many via CharacterAbilities)
        modelBuilder.Entity<Character>()
            .HasMany(c => c.Abilities)
            .WithMany(a => a.Characters)
            .UsingEntity(j => j.ToTable("CharacterAbilities"));

        // Character ↔ Magic (many-to-many via CharacterMagic)
        modelBuilder.Entity<Character>()
            .HasMany(c => c.Magics)
            .WithMany(m => m.Characters)
            .UsingEntity(j => j.ToTable("CharacterMagic"));

        // --- CharacterSkill (explicit join with Proficiency) ---
        modelBuilder.Entity<CharacterSkill>()
            .HasKey(cs => new { cs.CharacterId, cs.SkillId });

        modelBuilder.Entity<CharacterSkill>()
            .HasOne(cs => cs.Character)
            .WithMany(c => c.CharacterSkills)
            .HasForeignKey(cs => cs.CharacterId);

        modelBuilder.Entity<CharacterSkill>()
            .HasOne(cs => cs.Skill)
            .WithMany(s => s.CharacterSkills)
            .HasForeignKey(cs => cs.SkillId);

        // --- EquipmentSlot → Item (many-to-one, nullable for empty slots) ---
        modelBuilder.Entity<EquipmentSlot>()
            .HasOne(es => es.EquippedItem)
            .WithMany()
            .HasForeignKey(es => es.EquippedItemId)
            .OnDelete(DeleteBehavior.SetNull);

        // --- Door → Room (two FKs, Restrict to prevent cascade conflicts) ---
        modelBuilder.Entity<Door>()
            .HasOne(d => d.SourceRoom)
            .WithMany(r => r.Doors)
            .HasForeignKey(d => d.SourceRoomId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Door>()
            .HasOne(d => d.DestinationRoom)
            .WithMany()
            .HasForeignKey(d => d.DestinationRoomId)
            .OnDelete(DeleteBehavior.Restrict);

        base.OnModelCreating(modelBuilder);
    }
}
