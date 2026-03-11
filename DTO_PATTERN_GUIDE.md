# Quick Reference: DTO Pattern Implementation

## When to Use What

### Use `Character` (abstract)
- **Never instantiate directly** - it's abstract!
- Base class for all character types
- Contains common properties: Name, Class, Level, Hp, Equipment

### Use `BasicCharacter` (concrete)
```csharp
// When creating a simple character (no combat behavior)
Character newCharacter = new BasicCharacter()
{
    Name = "Alice",
    Class = "Warrior",
    Level = 1,
    Hp = 100,
    Equipment = "sword|shield"
};
```

### Use `CharacterBase` (abstract)
- Base class for combat entities
- Inherits from `Character`
- Adds: `Attack()` and abstract `PerformSpecialAction()`
- **All your character classes** (Wizard, Fighter, etc.) inherit from this

### Use `CharacterDto` (concrete)
- **Only for serialization**
- Used internally by file handlers
- You typically won't use this directly in your code

---

## Common Patterns

### Reading Characters from File
```csharp
// File handler does this internally:
// 1. Read CharacterDto from file
// 2. Map to Character using CharacterMapper.ToCharacter()
// 3. Return List<Character>

var fileHandler = new CsvFileHandler("input.csv");
List<Character> characters = fileHandler.ReadAll(); // Returns BasicCharacter instances
```

### Writing Characters to File
```csharp
// File handler does this internally:
// 1. Map Character to CharacterDto using CharacterMapper.ToDto()
// 2. Write CharacterDto to file

var fileHandler = new CsvFileHandler("input.csv");
fileHandler.WriteAll(characters); // Automatically converts to DTOs
```

### Creating Combat Characters
```csharp
// These all inherit from CharacterBase, which inherits from Character
var wizard = new Wizard("Gandalf", 10, 50, "staff|robe");
var fighter = new Fighter("Conan", 8, 100, "sword|shield");
var healer = new Healer("Elara", 5, 60, "staff|herbs");

// They can all be treated as IEntity or Character
List<IEntity> entities = new List<IEntity> { wizard, fighter, healer };
```

---

## Understanding the Hierarchy

```
Character (abstract)
├── BasicCharacter (concrete) ← For general use
└── CharacterBase (abstract) ← For combat entities
    ├── Wizard (concrete)
    ├── Fighter (concrete)
    ├── Healer (concrete)
    ├── Rogue (concrete)
    ├── Paladin (concrete)
    └── ... all other character classes
```

---

## Why This Design?

### ❌ Old Way (Violates SRP)
```csharp
// Character had to be concrete for CsvHelper
public class Character  // Concrete - can be instantiated by CsvHelper
{
    // Properties + business logic mixed with serialization needs
}
```

### ✅ New Way (Follows SRP)
```csharp
// Domain Model - Abstract, clean, focused on business logic
public abstract class Character
{
    // Business logic and properties
}

// Data Transfer - Concrete, simple, focused on serialization
public class CharacterDto
{
    // Just properties for file I/O
}

// Concrete Implementation - For general use when not in combat
public class BasicCharacter : Character
{
    // Concrete so it can be instantiated
}
```

---

## Key Rules

1. **Never use `new Character()`** - It's abstract!
2. **Use `new BasicCharacter()`** when you need a simple character
3. **Use character classes** (Wizard, Fighter, etc.) for combat entities
4. **File handlers automatically handle DTO conversion** - you don't need to
5. **CharacterDto is internal** - you rarely interact with it directly

---

## Migration Guide

If you have old code that did this:
```csharp
Character character = new Character(...);  // ❌ Won't compile anymore
```

Change it to:
```csharp
Character character = new BasicCharacter(...);  // ✅ Works!
```

If you have code using your character classes:
```csharp
var wizard = new Wizard(...);  // ✅ Still works perfectly!
var fighter = new Fighter(...);  // ✅ No changes needed!
```

---

## Still Maintains All Your Week 6 Requirements

✅ **DIP**: GameEngine depends on abstractions (IEntity, IFileHandler)
✅ **Abstract Class**: CharacterBase is abstract with shared implementation
✅ **Abstract Methods**: PerformSpecialAction() must be implemented
✅ **Derived Classes**: All character classes inherit from CharacterBase
✅ **Integration**: Everything works together seamlessly

**Plus added bonus:**
✅ **Proper SRP**: Domain models separated from serialization
✅ **Professional pattern**: Industry-standard DTO approach
✅ **Future-ready**: Prepared for database integration
