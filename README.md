# Week 3: Single Responsibility Principle (SRP) & LINQ

> **Template Purpose:** This template shows code that VIOLATES SRP (on purpose!). Your job is to refactor it into well-organized, single-responsibility classes.

---

## Overview

This week you'll learn your first SOLID principle: **Single Responsibility Principle (SRP)**. You'll refactor your code so each class has one job. You'll also learn LINQ (Language Integrated Query) to search and filter data efficiently. This is your first step toward professional code organization.

## Learning Objectives

By completing this assignment, you will:
- [ ] Understand and apply the Single Responsibility Principle
- [ ] Create separate classes for reading and writing data
- [ ] Use LINQ to query collections (`FirstOrDefault`, `Where`)
- [ ] Organize code into a logical folder structure

## Prerequisites

Before starting, ensure you have:
- [ ] Completed Week 2 assignment (or are using this template)
- [ ] Working CSV read/write with equipment arrays
- [ ] Basic understanding of classes and methods

## What's New This Week

| Concept | Description |
|---------|-------------|
| SRP | Each class should have only one reason to change |
| `CharacterReader` | Class dedicated to reading character data |
| `CharacterWriter` | Class dedicated to writing character data |
| LINQ `FirstOrDefault` | Find a single item matching a condition |
| LINQ `Where` | Filter a collection by condition |

---

## Assignment Tasks

### Task 1: Understand the Problem (SRP Violation)

**What to do:**
- Open `CharacterManager.cs` and read the comments at the top
- Notice how this ONE class does EVERYTHING: reading, writing, searching, menu handling
- This violates SRP - each class should have only ONE reason to change

**What's Already Provided:**
- `Models/Character.cs` - A proper data class (review this as an example of good SRP)
- `Services/CharacterReader.cs` - Stub class for reading (you'll implement this)
- `Services/CharacterWriter.cs` - Stub class for writing (you'll implement this)

**The Character class is done for you:**
```csharp
public class Character
{
    public string Name { get; set; }
    public string Profession { get; set; }
    public int Level { get; set; }
    public int HP { get; set; }
    public string[] Equipment { get; set; }
}
```

### Task 2: Implement CharacterReader Class

**What to do:**
- Open `Services/CharacterReader.cs` - the structure is there, you implement the logic
- Implement `ReadAll()` to read from CSV and return `List<Character>`
- Implement `FindByName()` using LINQ's `FirstOrDefault`

**LINQ is the key learning here!**
```csharp
// FirstOrDefault returns the first match, or null if none found
public Character FindByName(List<Character> characters, string name)
{
    return characters.FirstOrDefault(c => c.Name == name);
}

// Where returns all matches as a collection
public List<Character> FindByProfession(List<Character> characters, string profession)
{
    return characters.Where(c => c.Profession == profession).ToList();
}
```

### Task 3: Implement CharacterWriter Class

**What to do:**
- Open `Services/CharacterWriter.cs` - the structure is there, you implement the logic
- Implement `WriteAll()` to save all characters (replaces file)
- Implement `AppendCharacter()` to add one character (doesn't rewrite everything)

**Example:**
```csharp
public void WriteAll(List<Character> characters)
{
    var lines = characters.Select(c => FormatCharacter(c)).ToList();
    File.WriteAllLines(_filePath, lines);
}

public void AppendCharacter(Character character)
{
    string line = FormatCharacter(character);
    File.AppendAllText(_filePath, line + Environment.NewLine);
}
```

### Task 4: Update Menu with Find Character

**What to do:**
- Add "Find Character" option to menu
- Prompt for name, use LINQ to search
- Display result or "not found" message

---

## Stretch Goal (+10%)

**Enhanced Console Output**

Use a NuGet package to improve your console display:

```bash
dotnet add package Spectre.Console
```

Or use string interpolation with alignment:
```csharp
Console.WriteLine($"{"Name",-15} {"Profession",-10} {"Level",5}");
Console.WriteLine($"{character.Name,-15} {character.Profession,-10} {character.Level,5}");
```

---

## Menu Structure

Your menu should now include Find Character:
```
1. Display Characters
2. Find Character
3. Add Character
4. Level Up Character
0. Exit
```

---

## Project Structure

```
w3.console/
├── Program.cs                      # Entry point
├── CharacterManager.cs             # THE PROBLEM - violates SRP (refactor this!)
├── Models/
│   └── Character.cs                # PROVIDED - data class example
├── Services/
│   ├── CharacterReader.cs          # TODO - implement reading
│   └── CharacterWriter.cs          # TODO - implement writing
├── Interfaces/
│   ├── IInput.cs                   # For testing
│   └── IOutput.cs                  # For testing
└── Files/
    └── input.csv                   # Character data
```

---

## LINQ Quick Reference

```csharp
// Find first match (or null)
var hero = characters.FirstOrDefault(c => c.Name == "Hero");

// Filter to multiple matches
var warriors = characters.Where(c => c.Profession == "Warrior").ToList();

// Sort by property
var sorted = characters.OrderBy(c => c.Level).ToList();

// Get just names
var names = characters.Select(c => c.Name).ToList();
```

---

## Grading Rubric

| Criteria | Points | Description |
|----------|--------|-------------|
| SRP Implementation | 30 | CharacterReader and CharacterWriter have clear, single responsibilities |
| LINQ Implementation | 25 | FindByName uses LINQ correctly |
| File I/O Integration | 20 | Read/write works with refactored classes |
| Program Flow | 15 | Menu works with new Find Character option |
| Code Quality | 10 | Clean, readable, well-commented |
| **Total** | **100** | |
| **Stretch: Enhanced Output** | **+10** | Uses Spectre.Console or formatted strings |

---

## How This Connects to the Final Project

- SRP is the foundation of clean architecture used throughout the course
- The `Character` class evolves into your `Player` entity
- LINQ becomes essential for database queries in Weeks 9-12
- This class structure previews the separation used in the final project

**Next Week Preview:** In Week 4, you'll create an `IFileHandler` interface that combines CharacterReader and CharacterWriter. This lets you swap CSV for JSON without changing your business logic - the Open/Closed Principle!

---

## Tips

- Start by creating the Character class before refactoring
- Test each class independently before integrating
- LINQ methods are chainable: `characters.Where(...).OrderBy(...).ToList()`
- If you're stuck, review the in-class examples

---

## Submission

1. Commit your changes with a meaningful message
2. Push to your GitHub Classroom repository
3. Submit the repository URL in Canvas

---

## Resources

- [Single Responsibility Principle](https://www.freecodecamp.org/news/solid-principles-single-responsibility-principle-explained/)
- [LINQ Documentation](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/)
- [Spectre.Console](https://spectreconsole.net/)

---

## Need Help?

- Post questions in the Canvas discussion board
- Attend office hours
- Review the in-class repository for additional examplesles