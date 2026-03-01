# Week 5: Liskov Substitution (LSP) & Interface Segregation (ISP)

> **Template Purpose:** This template represents a working solution through Week 4. Use YOUR repo if you're caught up. Use this as a fresh start if needed.

---

## Overview

This week covers two more SOLID principles: **Liskov Substitution Principle (LSP)** and **Interface Segregation Principle (ISP)**. You'll fix a design problem where a "fat" interface forces classes to implement methods they can't use, then create smaller, focused interfaces for specific behaviors. This week also introduces a simple **GameEngine** - a class that runs your game loop.

## Learning Objectives

By completing this assignment, you will:
- [ ] Understand why LSP matters (substitutability)
- [ ] Apply ISP by creating focused interfaces
- [ ] Check if an object implements an interface using `is` keyword
- [ ] Create a simple game engine class

## Prerequisites

Before starting, ensure you have:
- [ ] Completed Week 4 assignment (or are using this template)
- [ ] Working IFileHandler with CSV and JSON support
- [ ] Understanding of interfaces

## What's New This Week

| Concept | Description |
|---------|-------------|
| LSP | Subtypes must be substitutable for their base types |
| ISP | Many small interfaces are better than one big interface |
| `is` keyword | Check if object implements an interface |
| `GameEngine` | Class that runs the main game loop |
| Behavior interfaces | `IFlyable`, `IShootable`, etc. |

---

## What's in This Template

This template introduces several new classes. **Don't panic** - review them to understand the structure:

```
ConsoleRPG/
├── Program.cs              # Entry point
├── Services/
│   └── GameEngine.cs       # NEW: Runs the game loop
├── Models/
│   ├── Character.cs        # Player character
│   ├── Ghost.cs            # Can fly (implements IFlyable)
│   └── Goblin.cs           # Cannot fly
└── Interfaces/
    ├── IEntity.cs          # Base interface for all entities
    └── IFlyable.cs         # NEW: Interface for flying entities
```

---

## Assignment Tasks

### Task 1: Understand the LSP Problem

**The Problem:**
The current `IEntity` interface has a `Fly()` method, but not all entities can fly. Goblins can't fly, so they throw an exception or do nothing - violating LSP.

**Example of LSP Violation:**
```csharp
public interface IEntity
{
    void Attack();
    void Fly();  // Problem: Not all entities can fly!
}

public class Goblin : IEntity
{
    public void Attack() { /* works */ }
    public void Fly() { throw new NotSupportedException(); } // LSP violation!
}
```

### Task 2: Fix the LSP Violation

**What to do:**
- Remove `Fly()` from `IEntity`
- Create a new `IFlyable` interface
- Have only flying entities implement `IFlyable`

**Example:**
```csharp
public interface IEntity
{
    void Attack();
    // No Fly() here!
}

public interface IFlyable
{
    void Fly();
}

public class Ghost : IEntity, IFlyable
{
    public void Attack() { /* ghost attack */ }
    public void Fly() { Console.WriteLine("Ghost floats through the air!"); }
}

public class Goblin : IEntity
{
    public void Attack() { /* goblin attack */ }
    // No Fly() - goblins can't fly, and that's okay!
}
```

### Task 3: Update GameEngine to Check for Interfaces

**What to do:**
- Use the `is` keyword to check if an entity can fly before calling Fly()

**Example:**
```csharp
public void ProcessEntity(IEntity entity)
{
    entity.Attack(); // All entities can attack

    // Only call Fly() if the entity can fly
    if (entity is IFlyable flyingEntity)
    {
        flyingEntity.Fly();
    }
}
```

### Task 4: Create Two New Classes with Behaviors

**What to do:**
- Create at least two new entity classes
- Design focused interfaces for their unique abilities
- Integrate them into the GameEngine

**Examples:**
```csharp
public interface IShootable
{
    void Shoot();
}

public class Archer : IEntity, IShootable
{
    public void Attack() { /* basic attack */ }
    public void Shoot() { Console.WriteLine("Archer fires an arrow!"); }
}
```

---

## Stretch Goal (+10%)

**Implement the Command Pattern**

Create commands that encapsulate actions:

```csharp
public interface ICommand
{
    void Execute();
}

public class AttackCommand : ICommand
{
    private IEntity _entity;
    public AttackCommand(IEntity entity) { _entity = entity; }
    public void Execute() { _entity.Attack(); }
}

// Usage in GameEngine:
var commands = new List<ICommand>();
commands.Add(new AttackCommand(goblin));
commands.Add(new FlyCommand(ghost));
foreach (var cmd in commands) cmd.Execute();
```

---

## Why This Matters

**LSP in Practice:**
```csharp
// This should work with ANY IEntity
void ProcessEntities(List<IEntity> entities)
{
    foreach (var entity in entities)
    {
        entity.Attack(); // Safe - all entities can attack
        // entity.Fly();  // NOT safe - not all entities can fly!
    }
}
```

**ISP in Practice:**
```csharp
// Small, focused interfaces = flexibility
public class Dragon : IEntity, IFlyable, IBreathable
{
    // Implements exactly what it needs
}
```

---

## Grading Rubric

| Criteria | Points | Description |
|----------|--------|-------------|
| LSP Fix | 30 | Removed Fly() from IEntity, created IFlyable |
| GameEngine Update | 20 | Correctly checks for IFlyable before calling Fly() |
| New Classes | 20 | Created 2+ classes with focused interfaces |
| ISP Compliance | 10 | Interfaces are small and focused |
| Integration | 10 | Everything works together in GameEngine |
| Code Quality | 10 | Clean, readable, well-commented |
| **Total** | **100** | |
| **Stretch: Command Pattern** | **+10** | Implemented command pattern for actions |

---

## How This Connects to the Final Project

- LSP ensures your entity hierarchy works correctly
- ISP creates the ability/behavior interfaces used throughout
- The GameEngine pattern continues through the final project
- These patterns make your code extensible without modification

---

## Tips

- Start by understanding the existing code before modifying
- Draw a diagram of your interfaces and classes
- Test with a simple scenario: process a list with Ghost and Goblin
- The `is` keyword is your friend for checking interface implementation

---

## Submission

1. Commit your changes with a meaningful message
2. Push to your GitHub Classroom repository
3. Submit the repository URL in Canvas

---

## Resources

- [Liskov Substitution Principle](https://stackify.com/solid-design-liskov-substitution-principle/)
- [Interface Segregation Principle](https://www.baeldung.com/cs/interface-segregation-principle)
- [C# `is` Keyword](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/is)
- [Command Pattern](https://refactoring.guru/design-patterns/command/csharp/example)

---

## Need Help?

- Post questions in the Canvas discussion board
- Attend office hours
- Review the in-class repository for additional examplesonal examples