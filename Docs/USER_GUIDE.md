# User Guide — W9 Console RPG

How to build, run, and use the application.

---

## Prerequisites

- **.NET 10 SDK** (or later)
- **Network access to WCTC SQL Server** (`bitsql.wctc.edu`)

Verify .NET is installed:

```bash
dotnet --version
```

---

## Build and Run

From the solution root:

```bash
cd ConsoleRpg
dotnet run --no-launch-profile
```

Or build first, then run:

```bash
dotnet build
dotnet run --project ConsoleRpg --no-launch-profile
```

---

## Database Setup

The app connects to the WCTC school SQL Server at `bitsql.wctc.edu`, database `w9_efcore_SDunn` (shared across all Net Frameworks modules).

**Connection string is loaded from config files**, not hardcoded:

1. `ConsoleRpg/appsettings.json` — committed placeholder (safe, no real credentials)
2. `ConsoleRpg/appsettings.Development.json` — real credentials, **gitignored**, created locally

`GameContext.OnConfiguring` reads `appsettings.json` then overlays `appsettings.Development.json` via `ConfigurationBuilder`. To run the app for the first time on a new machine, copy `appsettings.json`, rename to `appsettings.Development.json`, and replace the placeholder password with your real SQL credential.

**Lazy loading** is enabled via `UseLazyLoadingProxies()` — navigation properties (marked `virtual`) load automatically on first access without explicit `Include()` calls.

**First run:** EF Core migrations create the database automatically. If you need to manually apply migrations:

```bash
dotnet ef database update --project ConsoleRpgEntities --startup-project ConsoleRpg
```

---

## Menu Reference

On launch you'll see the main menu. Options 1-5 are W7 features (JSON-backed). Options 6-11 are W9 features (SQL Server-backed).

```
=== Main Menu ===
1. Start Combat
2. View Player
3. Reset Battle
4. Character Manager (W6)
5. W6 GameEngine Demo
--- W9: EF Core ---
6. Display Characters
7. Find Character
8. Add Character
9. Add Room
10. Level Up Character
11. Display Rooms
0. Exit
```

### W7 Features (JSON / FileContext)

| Option | What It Does |
|--------|-------------|
| **1. Start Combat** | Runs the turn-based combat loop. Player attacks monsters using LINQ-driven battle logic. Stats persist to `players.json`. |
| **2. View Player** | Displays the player's stats, ability scores, and equipped items. |
| **3. Reset Battle** | Restores all monsters to full HP and heals the player. Reloads from the original `monsters.json`. |
| **4. Character Manager (W6)** | Opens a sub-menu for CSV/JSON character file I/O (display, find, add, level up, switch format). |
| **5. W6 GameEngine Demo** | Runs the Week 6 entity-turn demonstration showing DIP with abstract classes. |

### W9 Features (SQL Server / GameContext)

| Option | What It Does |
|--------|-------------|
| **6. Display Characters** | Lists all characters from the database with their level and room assignment. Room loads via lazy loading proxy. |
| **7. Find Character** | Searches for a character by name (partial match). Uses LINQ `FirstOrDefault` with `Contains`. |
| **8. Add Character** | Creates a new character and associates it with an existing room. Prompts for name, level, and room ID. |
| **9. Add Room** | Creates a new room. Prompts for name and description. |
| **10. Level Up Character** | Finds a character by exact name and increments their level by 1. Demonstrates EF Core change tracking. |
| **11. Display Rooms** | Lists all rooms with their ID, name, and description. Useful before adding a character (you need the room ID). |

---

## Walkthrough: First Run

The database starts empty. Here's a typical first session:

### 1. Create a Room

Select **9. Add Room**

```
Enter room name: Tavern
Enter room description: A dimly lit tavern with a roaring fireplace.

Room 'Tavern' added to the game.
```

### 2. Check Your Rooms

Select **11. Display Rooms**

```
=== Rooms (EF Core - SQL Server) ===

  [1] Tavern - A dimly lit tavern with a roaring fireplace.
```

### 3. Add a Character

Select **8. Add Character**

```
Enter character name: Aldric
Enter character level: 1
Enter room ID for the character: 1

Character 'Aldric' added to Tavern.
```

### 4. Find the Character

Select **7. Find Character**

```
Enter character name to search: Ald

Found: Aldric (Level 1)
```

### 5. Level Up

Select **10. Level Up Character**

```
Enter character name: Aldric

Aldric is now level 2!
```

### 6. Verify Everything

Select **6. Display Characters**

```
=== Characters (EF Core - SQL Server) ===

  [1] Aldric (Level 2) - Room: Tavern
```

---

## Data Persistence

This application uses **two separate data backends** that coexist through the `IContext` abstraction:

| Feature Set | Backend | Storage | Persistence |
|-------------|---------|---------|-------------|
| W7 (options 1-3) | FileContext | JSON files (`players.json`, `monsters.json`, `items.json`) | Player stats saved on change; monsters read-only |
| W9 (options 6-11) | GameContext | SQL Server via EF Core | All changes persisted immediately via `SaveChanges()` |
| W6 (options 4-5) | IFileHandler | CSV/JSON files | Separate file I/O system |

The two contexts are completely independent. Combat data (JSON) and database data (SQL Server) don't interact.

---

## Migration Commands

If you need to create or update the database schema:

```bash
# Generate a new migration
dotnet ef migrations add MigrationName --project ConsoleRpgEntities --startup-project ConsoleRpg

# Apply pending migrations
dotnet ef database update --project ConsoleRpgEntities --startup-project ConsoleRpg

# View migration status
dotnet ef migrations list --project ConsoleRpgEntities --startup-project ConsoleRpg
```

Both `--project` and `--startup-project` flags are required because the solution uses a two-project architecture (entities live in `ConsoleRpgEntities`, but the startup configuration is in `ConsoleRpg`).
