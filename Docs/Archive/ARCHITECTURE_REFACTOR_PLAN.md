# Architecture Refactor Plan: Composition-Based Character System

## Current Problem

You have TWO competing hierarchies:
1. **Entity Types** (Player, NPC, Monster) - WHO the character is
2. **Character Classes** (Wizard, Fighter, Healer) - WHAT abilities they have

Currently, both are using inheritance, which doesn't allow mixing (can't have a Player who is a Wizard).

---

## Desired Outcome

A **Player** can be a **Wizard**
An **NPC** can be a **Fighter**
A **Monster** can be a **Healer**

---

## Solution: Use Composition

### New Hierarchy:

```
Character (abstract base)
├── Player
├── Npc
└── Monster

ICharacterClass (interface for abilities)
├── WizardClass
├── FighterClass
├── HealerClass
└── ... (all class types)

Character HAS-A CharacterClass (composition)
```

---

## Implementation Plan

### Step 1: Create ICharacterClass Interface

```csharp
public interface ICharacterClass
{
    string ClassName { get; }
    void Attack();
    void PerformSpecialAction();
    List<Type> GetCapabilities(); // Returns IFlyable, IDefendable, etc.
}
```

### Step 2: Convert Character Classes to Ability Providers

```csharp
public class WizardClass : ICharacterClass
{
    public string ClassName => "Wizard";
    
    public void Attack(Character character) =>
        Console.WriteLine($"{character.Name} hurls a crackling bolt of arcane energy!");
    
    public void PerformSpecialAction(Character character) =>
        Console.WriteLine($"{character.Name} casts a devastating fireball!");
    
    public List<Type> GetCapabilities() => new() { typeof(IFlyable) };
}
```

### Step 3: Update Character to Use Composition

```csharp
public abstract class Character : IEntity
{
    // Data properties
    public string Name { get; set; }
    public int Level { get; set; }
    public int Hp { get; set; }
    public string Equipment { get; set; }
    
    // Composition: Character HAS-A class
    public ICharacterClass CharacterClass { get; set; }
    
    // Delegate to class for behavior
    public void Attack() => CharacterClass.Attack(this);
    public void PerformSpecialAction() => CharacterClass.PerformSpecialAction(this);
}
```

### Step 4: Implement Entity Types

```csharp
public class Player : Character
{
    public Player(string name, ICharacterClass characterClass)
    {
        Name = name;
        CharacterClass = characterClass;
        // Player-specific properties
    }
}

public class Monster : Character
{
    public string Faction { get; set; } = "Hostile";
    
    public Monster(string name, ICharacterClass characterClass)
    {
        Name = name;
        CharacterClass = characterClass;
    }
}
```

### Step 5: Usage

```csharp
// Create a Player who is a Wizard
var playerWizard = new Player("Gandalf", new WizardClass())
{
    Level = 10,
    Hp = 50
};

// Create an NPC who is a Fighter
var npcFighter = new Npc("Guard", new FighterClass())
{
    Level = 5,
    Hp = 100
};

// Create a Monster who is a Healer (boss fight!)
var monsterHealer = new Monster("Evil Priest", new HealerClass())
{
    Level = 20,
    Hp = 200
};

// All can be used polymorphically
List<IEntity> entities = new() { playerWizard, npcFighter, monsterHealer };
```

---

## Benefits

✅ **Flexible**: Any entity type can have any character class
✅ **Maintainable**: Entity logic separate from class logic
✅ **Extensible**: Add new entity types OR classes independently
✅ **Testable**: Can test entity behavior and class behavior separately
✅ **SOLID**: Follows all principles properly

---

## What This Means for Week 6

**Option 1: Keep current architecture for Week 6**
- CharacterBase with Wizard, Fighter, etc. inheriting
- Meets Week 6 requirements
- Refactor later when you need Player/NPC/Monster

**Option 2: Implement composition now**
- More work upfront
- Better architecture from the start
- Makes final project easier

---

## Migration Path

### If choosing Option 2:

1. **Keep existing files** (don't delete yet)
2. **Create new structure** alongside
3. **Test thoroughly**
4. **Switch GameEngine** to use new structure
5. **Remove old files** once verified

### Time estimate:
- 1-2 hours for basic refactor
- Additional time for testing

---

## Recommendation

For **Week 6 submission**: Keep current architecture (it meets requirements)

For **final project**: Refactor to composition-based system

This gives you a working Week 6 assignment NOW, while planning for better architecture later.
