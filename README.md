# Week 6: Dependency Inversion (DIP) & Abstract Classes

> **Template Purpose:** This template represents a working solution through Week 5. Use YOUR repo if you're caught up. Use this as a fresh start if needed.

---

## Overview

This week completes your SOLID journey with the **Dependency Inversion Principle (DIP)**: high-level modules should depend on abstractions, not concrete implementations. You'll also learn about **abstract classes** - a way to share common code between related classes while enforcing that derived classes implement certain methods.

## Learning Objectives

By completing this assignment, you will:
- [ ] Understand and apply the Dependency Inversion Principle
- [ ] Create abstract classes with shared properties and methods
- [ ] Use abstract methods that derived classes must implement
- [ ] See how DIP makes code more testable and flexible

## Prerequisites

Before starting, ensure you have:
- [ ] Completed Week 5 assignment (or are using this template)
- [ ] Working GameEngine with multiple entity types
- [ ] Understanding of interfaces and the `is` keyword

## What's New This Week

| Concept | Description |
|---------|-------------|
| DIP | Depend on abstractions, not concretions |
| `abstract class` | A class that can't be instantiated directly |
| `abstract method` | A method that derived classes MUST implement |
| `CharacterBase` | Abstract base class for all characters |
| `override` | Keyword to implement abstract methods |

---

## Assignment Tasks

### Task 1: Understand DIP

**The Problem:**
GameEngine directly creates and uses concrete classes, making it hard to test or swap implementations.

**Before (violates DIP):**
```csharp
public class GameEngine
{
    private CsvFileHandler _fileHandler = new CsvFileHandler("input.csv"); // Concrete!
    private Goblin _enemy = new Goblin(); // Concrete!
}
```

**After (follows DIP):**
```csharp
public class GameEngine
{
    private IFileHandler _fileHandler; // Abstraction!
    private ICharacter _enemy; // Abstraction!

    public GameEngine(IFileHandler fileHandler, ICharacter enemy)
    {
        _fileHandler = fileHandler; // Injected!
        _enemy = enemy; // Injected!
    }
}
```

### Task 2: Create CharacterBase Abstract Class

**What to do:**
- Create `CharacterBase.cs` with common properties
- Add abstract methods for behaviors that differ by character type

**Example:**
```csharp
public abstract class CharacterBase : ICharacter
{
    public string Name { get; set; }
    public int HitPoints { get; set; }
    public int Level { get; set; }

    protected CharacterBase(string name, int hitPoints, int level)
    {
        Name = name;
        HitPoints = hitPoints;
        Level = level;
    }

    public virtual void Attack(ICharacter target)
    {
        Console.WriteLine($"{Name} attacks {target.Name}!");
    }

    // Derived classes MUST implement this
    public abstract void PerformSpecialAction();
}
```

### Task 3: Create Derived Classes

**What to do:**
- Have your character classes inherit from `CharacterBase`
- Implement the abstract `PerformSpecialAction` method

**Example:**
```csharp
public class Warrior : CharacterBase
{
    public Warrior(string name, int hitPoints, int level)
        : base(name, hitPoints, level) { }

    public override void PerformSpecialAction()
    {
        Console.WriteLine($"{Name} performs a powerful sword strike!");
    }
}

public class Mage : CharacterBase
{
    public Mage(string name, int hitPoints, int level)
        : base(name, hitPoints, level) { }

    public override void PerformSpecialAction()
    {
        Console.WriteLine($"{Name} casts a fireball!");
    }
}
```

### Task 4: Update GameEngine for DIP

**What to do:**
- GameEngine should receive dependencies through its constructor
- Work with abstractions (interfaces/abstract classes), not concrete types

---

## Stretch Goal (+10%)

**Add Additional Character Types**

Create 2+ additional character classes with unique special actions:
- `Healer` with a healing ability
- `Rogue` with a stealth ability
- Your own creative class

---

## Abstract vs Interface

| Feature | Interface | Abstract Class |
|---------|-----------|----------------|
| Can have code? | No (just signatures) | Yes (shared implementation) |
| Multiple inheritance? | Yes | No |
| When to use | Define a contract | Share code between related classes |

**Use interfaces when:** different class families need same behavior
**Use abstract classes when:** related classes share common code

---

## Grading Rubric

| Criteria | Points | Description |
|----------|--------|-------------|
| DIP Implementation | 30 | GameEngine depends on abstractions, not concretions |
| CharacterBase Class | 25 | Proper abstract class with shared properties/methods |
| Derived Classes | 25 | Character classes inherit and implement abstract methods |
| Integration | 10 | Everything works together in GameEngine |
| Code Quality | 10 | Clean, readable, well-commented |
| **Total** | **100** | |
| **Stretch: Additional Classes** | **+10** | 2+ additional character types with unique behaviors |

---

## How This Connects to the Final Project

- `CharacterBase` evolves into your `Player` and `Monster` base classes
- DIP prepares you for dependency injection in EF Core (Week 9+)
- Abstract classes are used throughout the final project
- This completes the SOLID foundation for the semester

---

## Looking Ahead: From IFileHandler to IContext

In Week 4, `IFileHandler` worked great for one entity type (Characters). But what happens when your game needs **multiple entity types**?

**The Problem:**
```csharp
// Week 4 approach - separate handlers for each type?
IFileHandler characterHandler = new JsonFileHandler("characters.json");
IFileHandler monsterHandler = new JsonFileHandler("monsters.json");
IFileHandler itemHandler = new JsonFileHandler("items.json");
// This gets messy fast!
```

**The Solution (Week 7):**
```csharp
// IContext manages ALL entity types in one place
public interface IContext
{
    List<Player> Players { get; set; }
    List<MonsterBase> Monsters { get; set; }
    List<Item> Items { get; set; }

    void Read();
    void Write(Player player);
    void Write(MonsterBase monster);
    int SaveChanges();  // Save everything at once!
}
```

**Why this matters for the midterm:**
- The midterm codebase uses `IContext` and `GameContext`
- `GameContext` is like a "fake DbContext" using JSON files
- Understanding this pattern is critical for the exam
- In Week 9, `IContext` becomes the real `DbContext` with EF Core

**The evolution:**
```
IFileHandler (Week 4)     →     IContext (Week 7)     →     DbContext (Week 9)
Single entity type              Multiple entity types        Real database
```

---

## Tips

- Abstract classes can have both implemented and abstract members
- Use `protected` for members only derived classes should access
- Call base constructor with `: base(...)` syntax
- The `override` keyword is required when implementing abstract methods

---

## Submission

1. Commit your changes with a meaningful message
2. Push to your GitHub Classroom repository
3. Submit the repository URL in Canvas

---

## Resources

- [Dependency Inversion Principle](https://stackify.com/solid-design-dependency-inversion-principle/)
- [Abstract Classes in C#](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/abstract-and-sealed-classes-and-class-members)
- [SOLID Principles Overview](https://www.freecodecamp.org/news/solid-principles-every-developer-should-know/)

---

## Need Help?

- Post questions in the Canvas discussion board
- Attend office hours
- Review the in-class repository for additional examples