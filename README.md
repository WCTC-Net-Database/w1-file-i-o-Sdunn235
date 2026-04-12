# Week 10: Table-Per-Hierarchy (TPH) Inheritance

> **Template Purpose:** This template represents a working solution through Week 9. Use YOUR repo if you're caught up. Use this as a fresh start if needed.

---

## Overview

This week you'll learn how Entity Framework Core handles **inheritance** using the Table-Per-Hierarchy (TPH) pattern. TPH stores all types in a single table and uses a **discriminator column** to distinguish between them. This is how your `Player` and `Monster` types can share a `Characters` table while maintaining their unique properties.

## Learning Objectives

By completing this assignment, you will:
- [ ] Understand the TPH inheritance pattern in EF Core
- [ ] Configure discriminator columns in `OnModelCreating`
- [ ] Create an `Ability` entity hierarchy with TPH
- [ ] Set up many-to-many relationships between characters and abilities
- [ ] Generate and apply migrations for new entities

## Prerequisites

Before starting, ensure you have:
- [ ] Completed Week 9 assignment (or are using this template)
- [ ] Working GameContext with Rooms and Characters
- [ ] Understanding of abstract classes from Week 6
- [ ] Successful EF Core migrations experience

## What's New This Week

| Concept | Description |
|---------|-------------|
| TPH (Table-Per-Hierarchy) | One table stores all types in hierarchy |
| Discriminator | Column that identifies the concrete type |
| `HasDiscriminator<T>()` | EF Core method to configure TPH |
| `HasValue<T>()` | Maps a type to a discriminator value |
| Many-to-Many | Relationship with join table |

---

## Assignment Tasks

### Task 1: Understand TPH in the Template

**Review the existing code:**
- Look at how `Character` is configured as an abstract class
- See how `Player` and `Goblin` inherit from `Character`
- Notice the discriminator configuration in `GameContext`

**Example Configuration:**
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Configure TPH for Character hierarchy
    modelBuilder.Entity<Character>()
        .HasDiscriminator<string>("Discriminator")
        .HasValue<Player>("Player")
        .HasValue<Goblin>("Goblin");
}
```

### Task 2: Create the Ability Hierarchy

**What to do:**
- Create an abstract `Ability` base class
- Create concrete ability classes (e.g., `PlayerAbility`, `GoblinAbility`)
- Configure TPH for the Ability hierarchy

**Ability.cs (Abstract Base):**
```csharp
public abstract class Ability
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    // Navigation property for many-to-many
    public virtual ICollection<Character> Characters { get; set; }
}
```

**PlayerAbility.cs:**
```csharp
public class PlayerAbility : Ability
{
    public int Shove { get; set; }
}
```

**GoblinAbility.cs:**
```csharp
public class GoblinAbility : Ability
{
    public int Taunt { get; set; }
}
```

### Task 3: Update Character with Abilities

**What to do:**
- Add a navigation property for abilities to the Character class
- This creates a many-to-many relationship

**Character.cs:**
```csharp
public abstract class Character : ICharacter
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Level { get; set; }

    public int RoomId { get; set; }
    public virtual Room Room { get; set; }

    // Many-to-many: Character can have multiple Abilities
    public virtual ICollection<Ability> Abilities { get; set; }

    public virtual void Attack(ICharacter target)
    {
        Console.WriteLine($"{Name} attacks {target.Name}!");
    }
}
```

### Task 4: Configure Many-to-Many in GameContext

**What to do:**
- Add `DbSet<Ability>` to GameContext
- Configure the many-to-many relationship with a join table
- Configure TPH for the Ability hierarchy

**GameContext.cs:**
```csharp
public class GameContext : DbContext
{
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Character> Characters { get; set; }
    public DbSet<Ability> Abilities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // TPH for Characters
        modelBuilder.Entity<Character>()
            .HasDiscriminator<string>("Discriminator")
            .HasValue<Player>("Player")
            .HasValue<Goblin>("Goblin");

        // TPH for Abilities
        modelBuilder.Entity<Ability>()
            .HasDiscriminator<string>("AbilityType")
            .HasValue<PlayerAbility>("PlayerAbility")
            .HasValue<GoblinAbility>("GoblinAbility");

        // Many-to-many relationship
        modelBuilder.Entity<Character>()
            .HasMany(c => c.Abilities)
            .WithMany(a => a.Characters)
            .UsingEntity(j => j.ToTable("CharacterAbilities"));

        base.OnModelCreating(modelBuilder);
    }
}
```

### Task 5: Generate and Apply Migrations

**Commands:**
```bash
# Add migration for Abilities table
dotnet ef migrations add AddAbilitiesTable

# Apply migration
dotnet ef database update

# (Optional) Add seed data migration
dotnet ef migrations add SeedAbilities

# Apply seed migration
dotnet ef database update
```

---

## How TPH Works in the Database

When you look at the database, you'll see:

**Characters Table:**
| Id | Discriminator | Name | Level | RoomId | Sneakiness |
|----|---------------|------|-------|--------|------------|
| 1 | Player | Hero | 5 | 1 | NULL |
| 2 | Goblin | Sneaky | 2 | 2 | 8 |

- The `Discriminator` column tells EF Core which type to create
- `Sneakiness` is NULL for Players (only Goblins have it)
- All types share the same table

---

## Stretch Goal (+10%)

**Execute an Ability**

Add a method to execute abilities during combat:

**Character.cs:**
```csharp
public virtual void ExecuteAbility(Ability ability)
{
    Console.WriteLine($"{Name} uses {ability.Name}!");
}
```

**In GameEngine (during attack):**
```csharp
if (player.Abilities.Any())
{
    var ability = player.Abilities.First();
    player.ExecuteAbility(ability);
}
```

---

## Project Structure

```
ConsoleRPG/
├── Program.cs
├── Services/
│   └── GameEngine.cs
├── Models/
│   ├── Characters/
│   │   ├── Character.cs        # Abstract base
│   │   ├── Player.cs           # Player : Character
│   │   └── Goblin.cs           # Goblin : Character
│   ├── Abilities/
│   │   ├── Ability.cs          # Abstract base
│   │   ├── PlayerAbility.cs    # PlayerAbility : Ability
│   │   └── GoblinAbility.cs    # GoblinAbility : Ability
│   └── Room.cs
└── Data/
    └── GameContext.cs          # TPH configuration
```

---

## Grading Rubric

| Criteria | Points | Description |
|----------|--------|-------------|
| Ability Hierarchy | 25 | Abstract base and concrete classes created |
| TPH Configuration | 25 | Discriminator configured correctly |
| Many-to-Many Setup | 25 | Character-Ability relationship works |
| Migrations | 15 | Successfully generated and applied |
| Code Quality | 10 | Clean, readable, follows patterns |
| **Total** | **100** | |
| **Stretch: Execute Ability** | **+10** | Ability execution during combat |

---

## How This Connects to the Final Project

- TPH is used for all character types (Players, Goblins, other monsters)
- TPH is used for abilities, equipment, and items
- Many-to-many relationships connect entities throughout the game
- This pattern enables polymorphic queries (`_context.Characters` returns all types)

---

## TPH vs Other Patterns

| Pattern | Description | Tables |
|---------|-------------|--------|
| TPH | All types in one table | 1 table, discriminator column |
| TPT | Each type has own table | Separate table per type, joins needed |
| TPC | Concrete classes only | One table per concrete class |

TPH is the default and most common pattern in EF Core.

---

## Tips

- Use `virtual` for navigation properties to enable lazy loading
- The discriminator column is created automatically by EF Core
- Use `.Include()` to eager load related abilities
- Check the generated migration SQL to understand the database structure

---

## Submission

1. Commit your changes with a meaningful message
2. Push to your GitHub Classroom repository
3. Submit the repository URL in Canvas

---

## Resources

- [EF Core Inheritance](https://learn.microsoft.com/en-us/ef/core/modeling/inheritance)
- [Many-to-Many Relationships](https://learn.microsoft.com/en-us/ef/core/modeling/relationships/many-to-many)
- [Configuring TPH](https://learn.microsoft.com/en-us/ef/core/modeling/inheritance#table-per-hierarchy-and-discriminator-configuration)

---

## Need Help?

- Post questions in the Canvas discussion board
- Attend office hours
- Review the in-class repository for additional examples