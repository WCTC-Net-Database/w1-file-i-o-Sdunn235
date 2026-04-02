# Week 9: Entity Framework Core Introduction

## Completion Notes

> **Status:** All tasks complete — ready for submission.

| Item | Status |
|------|--------|
| Option A (continued own repo) | Used |
| Task 1: EF Core Setup | Done |
| Task 2: Create GameContext | Done |
| Task 3: Migrations | Done |
| Task 4: Add Room | Done |
| Task 5: Add Character | Done |
| Task 6: Find Character | Done |
| Stretch Goal: Level Up Character (+10%) | Done |
| Bonus: Display Rooms | Done |

**Architecture approach:** Rather than starting from the template, this project evolved the existing W7 architecture. `IContext` was refactored so both `FileContext` (JSON) and `GameContext` (EF Core DbContext) implement the same contract. W7 combat features and W9 database features coexist through dependency injection — business logic never knows which backend is in use.

**See also:** [User Guide](USER_GUIDE.md) | [Architecture Guide](ARCHITECTURE_GUIDE.md)

---

> **Template Purpose:** This template represents a working solution through Week 8. Use YOUR repo if you're caught up. Use this as a fresh start if needed.

---

## How to Use This Template

### Option A: Continue Your Own Repository (Recommended)
If you're caught up from the midterm:
1. **DO NOT** clone this template
2. Continue working in your existing repository
3. Follow this README to add EF Core to YOUR project
4. Reference the template code if you get stuck

### Option B: Fresh Start (If Behind)
If you've fallen behind or your code has issues:
1. Accept this GitHub Classroom assignment
2. This becomes your new "main" repository going forward
3. Complete this week's tasks in the template

> **Note:** The jump to EF Core is significant. Starting fresh with a known-good foundation can sometimes be better than debugging EF Core + existing problems.

---

## Overview

This week introduces **Entity Framework Core** - Microsoft's modern object-relational mapper (ORM) for .NET. You'll transition from file-based storage (CSV/JSON) to a real SQL Server database.

### Remember Weeks 4 and 7? This is the Payoff!

You've been building toward this moment:

```
Week 4:  IFileHandler                 Week 7:  IContext                  Week 9:  DbContext
├── ReadAll()                         ├── Players (List)                 ├── Players (DbSet)
├── WriteAll()                        ├── Monsters (List)                ├── Monsters (DbSet)
├── Find methods              →       ├── Read()                  →      ├── LINQ queries
└── CsvFileHandler                    ├── Write(entity)                  ├── Add(entity)
    JsonFileHandler                   └── SaveChanges()                  └── SaveChanges()
```

**The pattern is the same:**
- Week 4: You swapped CSV for JSON without changing business logic
- Week 7: `IContext` added `SaveChanges()` - just like DbContext!
- Week 9: `DbContext` is the real deal - same pattern, real database

Your business logic doesn't care where data comes from. Whether it's CSV files, JSON files, or SQL Server - the pattern remains: **depend on abstractions, swap implementations.**

## Learning Objectives

By completing this assignment, you will:
- [x] Set up Entity Framework Core with SQL Server
- [x] Create a DbContext to manage database connections
- [x] Generate and apply database migrations
- [x] Perform basic CRUD operations (Create, Read, Update, Delete)
- [x] Use LINQ with EF Core to query the database

## Prerequisites

Before starting, ensure you have:
- [x] Completed Week 8 midterm (or are using this template)
- [x] SQL Server installed (LocalDB or full SQL Server)
- [x] Understanding of interfaces and SOLID principles
- [x] Working LINQ knowledge

## What's New This Week

| Concept | Description |
|---------|-------------|
| Entity Framework Core | ORM that maps C# classes to database tables |
| `DbContext` | Class that manages database connections and operations |
| `DbSet<T>` | Represents a table in the database |
| Migrations | Version control for your database schema |
| Connection String | Configuration for connecting to SQL Server |

---

## Assignment Tasks

### Task 1: Setup Entity Framework Core

**What to do:**
- Install the required NuGet packages

**Commands:**
```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

**Verify installation:**
```bash
dotnet list package
```

### Task 2: Create the GameContext

**What to do:**
- Create `GameContext.cs` with `DbSet` properties for your entities
- Configure the connection string

**Example:**
```csharp
using Microsoft.EntityFrameworkCore;

public class GameContext : DbContext
{
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Character> Characters { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ConsoleRPG;Trusted_Connection=True;");
    }
}
```

### Task 3: Generate and Apply Migrations

**What to do:**
- Install the EF Core CLI tools (if not already installed)
- Generate the initial migration
- Apply the migration to create the database

**Commands:**
```bash
# Install EF Core tools globally
dotnet tool install --global dotnet-ef

# Verify installation
dotnet ef

# Generate migration
dotnet ef migrations add InitialCreate

# Apply migration
dotnet ef database update
```

### Task 4: Implement Add Room Feature

**What to do:**
- Add a menu option to create a new room
- Implement the `AddRoom()` method in GameEngine

**Example:**
```csharp
public void AddRoom()
{
    Console.Write("Enter room name: ");
    var name = Console.ReadLine();

    Console.Write("Enter room description: ");
    var description = Console.ReadLine();

    var room = new Room
    {
        Name = name,
        Description = description
    };

    _context.Rooms.Add(room);
    _context.SaveChanges();

    Console.WriteLine($"Room '{name}' added to the game.");
}
```

### Task 5: Implement Add Character Feature

**What to do:**
- Add a menu option to create a new character
- Associate the character with a room

**Example:**
```csharp
public void AddCharacter()
{
    Console.Write("Enter character name: ");
    var name = Console.ReadLine();

    Console.Write("Enter character level: ");
    var level = int.Parse(Console.ReadLine());

    Console.Write("Enter room ID for the character: ");
    var roomId = int.Parse(Console.ReadLine());

    var room = _context.Rooms.Find(roomId);
    if (room == null)
    {
        Console.WriteLine("Room not found!");
        return;
    }

    var character = new Character
    {
        Name = name,
        Level = level,
        RoomId = roomId
    };

    _context.Characters.Add(character);
    _context.SaveChanges();

    Console.WriteLine($"Character '{name}' added to {room.Name}.");
}
```

### Task 6: Implement Find Character Feature

**What to do:**
- Add a menu option to search for characters
- Use LINQ to query the database

**Example:**
```csharp
public void FindCharacter()
{
    Console.Write("Enter character name to search: ");
    var name = Console.ReadLine();

    var character = _context.Characters
        .FirstOrDefault(c => c.Name.Contains(name));

    if (character != null)
    {
        Console.WriteLine($"Found: {character.Name} (Level {character.Level})");
    }
    else
    {
        Console.WriteLine("Character not found.");
    }
}
```

---

## Menu Structure

Your menu should include these new options:
```
1. Display Characters
2. Find Character
3. Add Character
4. Add Room
5. Level Up Character
0. Exit
```

---

## Project Structure

This template uses a **two-project architecture** (same as Week 7/8):

```
ConsoleRpgFinal.sln
│
├── ConsoleRpg/                        # UI & Game Logic
│   ├── Program.cs                     # Entry point
│   ├── Startup.cs                     # Dependency injection
│   ├── GameEngine.cs                  # Game logic with EF Core
│   └── appsettings.json               # Connection string
│
└── ConsoleRpgEntities/                # Data & Models (EF Core lives here!)
    ├── Data/
    │   └── GameContext.cs             # EF Core DbContext
    ├── Models/
    │   ├── Character.cs               # Character entity
    │   └── Room.cs                    # Room entity
    └── Migrations/                    # Auto-generated by EF
```

> **Important:** When running EF commands, specify the correct project:
> ```bash
> dotnet ef migrations add InitialCreate --project ConsoleRpgEntities
> dotnet ef database update --project ConsoleRpgEntities --startup-project ConsoleRpg
> ```

---

## Stretch Goal (+10%)

**Update Character Level**

Add functionality to find and update a character's level:

```csharp
public void LevelUpCharacter()
{
    Console.Write("Enter character name: ");
    var name = Console.ReadLine();

    var character = _context.Characters
        .FirstOrDefault(c => c.Name == name);

    if (character != null)
    {
        character.Level++;
        _context.SaveChanges();
        Console.WriteLine($"{character.Name} is now level {character.Level}!");
    }
    else
    {
        Console.WriteLine("Character not found.");
    }
}
```

---

## Connection String Reference

Find connection string formats at [connectionstrings.com/sql-server](https://www.connectionstrings.com/sql-server/)

**LocalDB (recommended for development):**
```
Server=(localdb)\mssqllocaldb;Database=ConsoleRPG;Trusted_Connection=True;
```

**SQL Server Express:**
```
Server=.\SQLEXPRESS;Database=ConsoleRPG;Trusted_Connection=True;
```

---

## Grading Rubric

| Criteria | Points | Description |
|----------|--------|-------------|
| EF Core Setup | 20 | Packages installed, context created |
| Migrations | 20 | Successfully generated and applied |
| Add Room | 20 | Room creation works and persists |
| Add Character | 20 | Character creation with room association |
| Find Character | 10 | LINQ query returns correct results |
| Code Quality | 10 | Clean, readable, well-organized |
| **Total** | **100** | |
| **Stretch: Level Up** | **+10** | Update character level functionality |

---

## How This Connects to the Final Project

- `GameContext` is the foundation for all database operations
- The Room and Character entities expand into the full game world
- LINQ queries become more complex with related entities
- This pattern (DbContext + entities + LINQ) is used throughout the final project

---

## Tips

- Use SQL Server Object Explorer in Visual Studio to view your database
- `SaveChanges()` must be called to persist data
- Use `Find()` for primary key lookups, `FirstOrDefault()` for other queries
- Check migration files in the `Migrations` folder to see what SQL will run

---

## Submission

1. Commit your changes with a meaningful message
2. Push to your GitHub Classroom repository
3. Submit the repository URL in Canvas

---

## Resources

- [EF Core Getting Started](https://learn.microsoft.com/en-us/ef/core/get-started/overview/first-app)
- [EF Core DbContext](https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/)
- [EF Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [Connection Strings](https://www.connectionstrings.com/sql-server/)

---

## Need Help?

- Post questions in the Canvas discussion board
- Attend office hours
- Review the in-class repository for additional examples