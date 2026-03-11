# Week 7: Midterm Preparation

> **Template Purpose:** This template provides the codebase you'll work with during the midterm exam. Review it thoroughly before the exam.

---

## Overview

**There is no assignment this week.** Instead, focus on reviewing this code and preparing for the in-class midterm exam. The exam will test your ability to modify and extend an existing codebase using the concepts from Weeks 1-6.

## What to Study

The midterm will assess your understanding of:

| Concept | Where You Learned It |
|---------|---------------------|
| File I/O (JSON) | Weeks 1-4 |
| LINQ queries | Weeks 3, 7 |
| Interfaces | Weeks 4-5 |
| Abstract classes | Week 6 |
| SOLID principles | Weeks 3-6 |
| Inheritance | Weeks 5-6 |

### The IContext Pattern (Key Concept!)

Remember `IFileHandler` from Week 4? This template introduces `IContext` - the next evolution:

```
Week 4: IFileHandler              Week 7: IContext
├── ReadAll()                     ├── Players (List)
├── WriteAll()                    ├── Monsters (List)
├── FindByName()         →        ├── Items (List)
├── FindByProfession()            ├── Read()
└── AppendCharacter()             ├── Write(entity)
                                  └── SaveChanges()
```

**Why the change?**
- `IFileHandler` works with one entity type (Characters)
- `IContext` works with multiple entity types (Players, Monsters, Items)
- `SaveChanges()` mimics how databases work - you make changes in memory, then save them all at once

**Coming in Week 9:** `IContext` becomes `DbContext` with real database support!

---

## What's in This Template

This template is a culmination of everything so far. Notice it uses a **two-project architecture**:

```
ConsoleRpgFinal.sln                    # Solution file
│
├── ConsoleRpg/                        # UI & Game Logic Project
│   ├── ConsoleRpg.csproj
│   ├── Program.cs                     # Entry point with DI setup
│   ├── Startup.cs                     # Dependency injection configuration
│   ├── GameEngine.cs                  # Main game loop
│   ├── appsettings.json               # Configuration settings
│   │
│   ├── Services/                      # Business logic
│   │   ├── BattleService.cs           # Combat calculations
│   │   ├── IBattleService.cs
│   │   ├── PlayerService.cs           # Player operations
│   │   └── IPlayerService.cs
│   │
│   ├── UI/                            # User interface
│   │   ├── ConsoleGameUi.cs
│   │   └── IGameUi.cs
│   │
│   ├── Decorators/                    # Decorator pattern example
│   │   └── AutoSavePlayerServiceDecorator.cs
│   │
│   └── Helpers/
│       └── ConfigurationHelper.cs
│
└── ConsoleRpgEntities/                # Data & Models Project
    ├── ConsoleRpgEntities.csproj
    │
    ├── Data/                          # Data access layer
    │   ├── GameContext.cs             # Primary data context
    │   ├── IContext.cs                # Context interface (DIP)
    │   ├── IEntityDao.cs              # Generic DAO interface
    │   ├── PlayerDao.cs               # Player data operations
    │   └── MonsterDao.cs              # Monster data operations
    │
    ├── Models/                        # Entity classes
    │   ├── Player.cs                  # Player entity
    │   ├── MonsterBase.cs             # Abstract monster base
    │   ├── Goblin.cs                  # Goblin implementation
    │   ├── Dragon.cs                  # Dragon implementation
    │   ├── Item.cs                    # Equipment/items
    │   ├── AbilityScores.cs           # Character stats
    │   └── Attribute.cs               # Attribute enum
    │
    ├── Interfaces/
    │   └── IMonster.cs
    │
    └── Files/                         # JSON data files
        ├── players.json
        ├── monsters.json
        └── items.json
```

### Why Two Projects?

This separation follows the **Separation of Concerns** principle:

- **ConsoleRpg**: Handles user interaction, game flow, and business logic
- **ConsoleRpgEntities**: Contains data models and data access (could be reused by other UIs)

The ConsoleRpg project references ConsoleRpgEntities, not the other way around.

---

## What the Exam Might Ask You to Do

During the exam, you may be asked to:

### 1. Add a New Monster Type
- Create a new class inheriting from `MonsterBase`
- Add it to `monsters.json`
- Load it in `DataContext`

### 2. Add a New Room
- Create a room in the data
- Connect it to existing rooms
- Make sure players can navigate to it

### 3. Enhance Combat
- Modify `BattleService.cs` to add new attack types
- Use equipped items to modify damage
- Add special abilities

### 4. Update the Menu
- Add new menu options
- Integrate with existing game logic

---

## Practice Exercise

Try these modifications to prepare:

**Add a Dragon Monster:**
1. Create `Dragon.cs` inheriting from `MonsterBase`
2. Give it a `BreathFire()` special ability
3. Add a dragon to `monsters.json`
4. Load it in the DataContext

**Add Item-Based Combat:**
1. Look at `BattleService.cs`
2. Calculate attack bonus from equipped items
3. Calculate defense bonus from equipped armor
4. Use LINQ to sum up item bonuses

---

## LINQ Examples for Combat

```csharp
// Sum attack bonuses from equipped weapons
int attackBonus = player.Items
    .Where(item => item.IsEquipped && item.Type == "Weapon")
    .Sum(item => item.AttackBonus);

// Sum defense from equipped armor
int defenseBonus = player.Items
    .Where(item => item.IsEquipped && item.Type == "Armor")
    .Sum(item => item.DefenseBonus);
```

---

## How to Prepare

1. **Review the code** - Understand every file in this template
2. **Practice modifications** - Try adding monsters, items, rooms
3. **Know your LINQ** - `Where`, `FirstOrDefault`, `Sum`, `Select`
4. **Understand inheritance** - How to create and extend abstract classes
5. **Review SOLID** - Know why code is structured this way

---

## Exam Day Tips

- Read the requirements carefully before coding
- Start with the simplest task
- Test frequently - don't write everything then test
- Use the patterns you see in existing code
- Ask for clarification if requirements are unclear

---

## Resources

- [LINQ Documentation](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/)
- [Abstract Classes](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/abstract-and-sealed-classes-and-class-members)
- [Working with JSON](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-overview)

---

## Questions?

- Post in Canvas discussion board
- Attend office hours
- Ask during class

Good luck with your preparation!