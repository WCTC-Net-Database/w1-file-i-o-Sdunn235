# ConsoleRPG — Brain Dump & Planning
> Week 5: LSP & ISP — Full Design Plan
> Last Updated: 2026-02-27

---

## Core Design Philosophy

> "Anything a Player can do, an NPC can do too."

All entities — Player and NPC alike — derive from `Character`.
Capabilities (flying, shooting, swimming, defending) are granted by interfaces, not by class position.
`GameEngine` works against `IEntity` exclusively — it never cares if something is a `Player` or `Monster`.
**Races** define *what you are*. **Classes** define *your role*. **Interfaces** define *what you can do*.

---

## Current File Structure (W5)

```
ConsoleRPG/
??? Program.cs
??? BrainDump/
?   ??? BRAINDUMP.md                                    ? this file
??? Services/
?   ??? GameEngine.cs                                   ? W5: runs game loop, uses 'is' keyword
?   ??? CharacterUI.cs
?   ??? CharacterWriter.cs
?   ??? CharacterReader.cs
?   ??? MenuService.cs
?   ??? CsvFileHandler.cs
?   ??? JsonFileHandler.cs
?   ??? Commands/
?       ??? AttackCommand.cs                            ? W5 Stretch: Command Pattern
?       ??? DefendCommand.cs                            ? W5 Stretch
?       ??? FlyCommand.cs                               ? W5 Stretch
?       ??? ShootCommand.cs                             ? W5 Stretch
?       ??? SwimCommand.cs                              ? W5 Stretch
??? Models/
?   ??? Characters/
?   ?   ??? Character.cs                                ? concrete base (all entities)
?   ?   ??? CharacterMap.cs                             ? CsvHelper column mapping
?   ?   ??? Players/
?   ?   ?   ??? Player.cs                               ? user-controlled (future)
?   ?   ??? Npcs/
?   ?       ??? Npc.cs                                  ? abstract, has Faction
?   ?       ??? Monsters/
?   ?       ?   ??? Monster.cs                          ? abstract, Faction = "Hostile"
?   ?       ?   ??? Ghost.cs                            ? IEntity, IFlyable
?   ?       ?   ??? Goblin.cs                           ? IEntity
?   ?       ?   ??? Troll.cs                            ? IEntity, ISwimmable
?   ?       ??? Townspeople/
?   ?           ??? Townsperson.cs                      ? abstract, Faction = "Neutral"
?   ?           ??? Races/
?   ?               ??? Human.cs                        ? no innate interfaces (pure versatility)
?   ?               ??? Elf.cs                          ? IFlyable (racial trait)
?   ?               ??? Dwarf.cs                        ? IDefendable (racial trait)
?   ?               ??? Orc.cs                          ? IEntity (racial trait — battle-ready)
?   ?               ??? Halfling.cs                     ? ISwimmable (racial trait)
?   ??? Classes/                                        ? RPG class templates (not C# classes!)
?       ??? Archer.cs                                   ? IEntity, IShootable
?       ??? Fighter.cs                                  ? IEntity, IDefendable
?       ??? Wizard.cs                                   ? IEntity, IFlyable
?       ??? Rogue.cs                                    ? IEntity
?       ??? Cleric.cs                                   ? IEntity, IDefendable
?       ??? Knight.cs                                   ? IEntity, IDefendable
?       ??? Blacksmith.cs                               ? IEntity, IDefendable
?       ??? Ranger.cs                                   ? IEntity, IShootable, ISwimmable
??? Interfaces/
?   ??? IFileHandler.cs                                 ? W4, carried forward
?   ??? IEntity.cs                                      ? W5: clean (no Fly — LSP fix)
?   ??? IFlyable.cs                                     ? W5
?   ??? IShootable.cs                                   ? W5
?   ??? ISwimmable.cs                                   ? W5
?   ??? IDefendable.cs                                  ? W5
?   ??? ICommand.cs                                     ? W5 Stretch
??? Input/
    ??? input.csv
    ??? input.json
```

---

## Character Hierarchy

```
Character (concrete — keeps CsvHelper happy)
?   string Name, Class, Equipment
?   int Level, Hp
?
??? Player (Players/Player.cs)
?       ? User-controlled
?       ? Can implement any interface: IEntity, IFlyable, IShootable, etc.
?
??? Npc (abstract — Npcs/Npc.cs)
    ?   string Faction
    ?
    ??? Monster (abstract — Npcs/Monsters/Monster.cs)
    ?   Faction = "Hostile"
    ?   ??? Ghost     ? IEntity, IFlyable
    ?   ??? Goblin    ? IEntity
    ?   ??? Troll     ? IEntity, ISwimmable
    ?
    ??? Townsperson (abstract — Npcs/Townspeople/)
        Faction = "Neutral" by default (can be Hostile or Friendly)
        Townspeople CAN implement IEntity — they fight if needed!
        ??? Races/
            ??? Human     ? (no innate interfaces — pure versatility)
            ??? Elf       ? IFlyable
            ??? Dwarf     ? IDefendable
            ??? Orc       ? IEntity
            ??? Halfling  ? ISwimmable
```

**Key rule:** Hierarchy = *what you are*. Interfaces = *what you can do*.
An Orc Blacksmith gets IEntity (racial) + IDefendable (class). A Human Wizard
gets IEntity + IFlyable (both from class). Stack freely — ISP keeps it clean.

---

## Models/Classes — RPG Class Templates

`Models/Classes/` holds RPG class templates (like D&D classes), not C# classes in the OOP sense.
These will eventually power a character creation system where a `Player` picks a `CharacterClass`.
Each class in the data files (CSV/JSON) has a matching class here.

| Class        | Interfaces                          | Data File Source     |
|--------------|-------------------------------------|----------------------|
| `Archer`     | IEntity, IShootable                 | CSV + JSON           |
| `Fighter`    | IEntity, IDefendable                | CSV + JSON           |
| `Wizard`     | IEntity, IFlyable                   | CSV + JSON           |
| `Rogue`      | IEntity                             | CSV + JSON           |
| `Cleric`     | IEntity, IDefendable                | CSV + JSON           |
| `Knight`     | IEntity, IDefendable                | CSV + JSON           |
| `Blacksmith` | IEntity, IDefendable                | CSV + JSON           |
| `Ranger`     | IEntity, IShootable, ISwimmable     | CSV + JSON           |

---

## Interface Design (ISP — All Small & Focused)

### W5 Built Interfaces

| Interface     | Members                     | Implementors (classes + races)                        |
|---------------|-----------------------------|-------------------------------------------------------|
| `IEntity`     | `Name { get; }`, `Attack()` | Ghost, Goblin, Troll, Archer, Fighter, Wizard, Rogue, Cleric, Knight, Blacksmith, Ranger, Orc |
| `IFlyable`    | `Fly()`                     | Ghost, Wizard, Elf                                    |
| `IShootable`  | `Shoot()`                   | Archer, Ranger                                        |
| `ISwimmable`  | `Swim()`                    | Troll, Ranger, Halfling                               |
| `IDefendable` | `Defend()`                  | Fighter, Cleric, Knight, Blacksmith, Dwarf            |
| `ICommand`    | `Execute()`                 | AttackCommand, DefendCommand, FlyCommand, ShootCommand, SwimCommand |

### Brain Dump Interfaces (Future Weeks)

| Interface     | Members                                        | Intended Implementors              |
|---------------|------------------------------------------------|------------------------------------|
| `IBreakable`  | `void Break()`, `bool IsBroken { get; }`       | Barrel, Crate, Box, Bin, Tree, Rock |
| `IOpenable`   | `void Open()`, `void Close()`, `bool IsOpen`   | Door, Chest, Dungeon, Castle, Hut  |
| `ILockable`   | `void Lock()`, `void Unlock()`, `bool IsLocked`| Door, Chest                        |
| `IDrinkable`  | `void Drink()`                                 | Potion, Flask                      |
| `IReadable`   | `void Read()`                                  | Book, Scroll, Sign                 |
| `IEquippable` | `void Equip()`, `void Unequip()`               | Weapon, Armor, Accessory           |
| `IWieldable`  | `void Wield()`                                 | Weapon (held in hand specifically) |
| `IUsable`     | `void Use()`                                   | Key, Lockpick, generic fallback    |

> **Note on IDrinkable vs IUsable:** These stay separate. A `Key` is IUsable but
> NOT IDrinkable. A `Potion` is IDrinkable. ISP keeps the verbs honest.

> **Note on IOpenable vs IBreakable:** A `Crate` is IBreakable only (smash it open).
> A `Chest` is both IOpenable and IBreakable (open normally OR force it).
> A `Barrel` is IBreakable only. A `Door` is IOpenable only (for now).
> LSP is safe — no subtype fakes an interface it can't support.

---

## World Building (Brain Dump — Future Weeks)

### Items
Everything that goes into a character's inventory is an `Item`.

```
Item (abstract)
?   string Name, Description
?   int Weight                          ? future stub
?
??? Consumable (abstract)               ? used up when consumed
?   ??? Potion        ? IDrinkable
?   ??? Food          ? IDrinkable
?   ??? Scroll        ? IReadable
?
??? Gear (abstract)                     ? equipped to character slots
?   ??? Weapon        ? IEquippable, IWieldable
?   ??? Armor         ? IEquippable
?   ??? Accessory     ? IEquippable     (rings, amulets, cloaks)
?
??? ReadableItem (abstract)             ? read but not consumed
?   ??? Book          ? IReadable
?
??? Tool (abstract)                     ? used for actions, not consumed
    ??? Key           ? IUsable
    ??? Lockpick      ? IUsable
```

**Container edge case:** Bags and pouches go in inventory AND hold items.
That's a Container that's also an Item. Keep simple for now — revisit later.

### World Objects
Things that exist in the world but do NOT go in inventory.

```
WorldObject (abstract)
?   string Name, Description
?
??? Container (abstract)                ? holds items, lives in world
?   ??? Chest         ? IOpenable, IBreakable
?   ??? Crate         ? IBreakable only (smash open, can't open like a chest)
?   ??? Barrel        ? IBreakable only
?   ??? Box           ? IBreakable only
?   ??? Bin           ? IBreakable only
?
??? Door              ? IOpenable, ILockable (future)
?
??? NaturalFeature (abstract)           ? environmental, not man-made
?   ??? Tree          ? IBreakable (chop it)
?   ??? Rock          ? IBreakable (mine it)
?   ??? Water         ? no interaction yet, just exists
?
??? Structure (abstract)                ? buildings and locations
    ??? Dungeon       ? IOpenable
    ??? Castle        ? IOpenable
    ??? Hut           ? IOpenable
```

### Biomes
Biomes are contexts/regions, not entities. A `Biome` does NOT implement `IEntity`.
They hold metadata about what `WorldObject` types naturally appear in them.
Instances of those objects are owned by the world map (future).

```
Biome (abstract)
??? Forest    ? NaturalFeatures: Tree, Rock, Mushroom
??? Swamp     ? NaturalFeatures: Water, DeadTree, Vine
??? Ocean     ? NaturalFeatures: Water, Coral, Rock
??? Desert    ? NaturalFeatures: Rock, Cactus, Sand
??? Arctic    ? NaturalFeatures: Ice, Snow, Rock
```

> Biome = class, not enum. Each `Biome` can carry its own logic (OCP-friendly).

---

## SOLID Compliance Summary

| Principle | How It's Applied |
|-----------|-----------------|
| **S**RP | `GameEngine` runs loop; `CharacterUI` handles display; each class owns one concern |
| **O**CP | New entity types slot in without touching `GameEngine`; new items don't touch `Character` |
| **L**SP | Every subtype substitutes safely — Character ? Npc ? Monster ? Ghost is a fully safe chain |
| **I**SP | 6 small behavior interfaces (1–2 members each); no class implements what it can't do |
| **D**IP | `GameEngine` depends on `IEntity` abstraction, never on `Ghost` or `Goblin` directly |

---

## W5 Build Order (Completed)

- [x] Rename W4Ocp ? W5SolidLsp (namespaces + .csproj + .sln)
- [x] Create `Models/Characters/` directory structure
- [x] Move `Character.cs` + `CharacterMap.cs` to `Models/Characters/`
- [x] Create `Player.cs`
- [x] Create `Npc.cs` (abstract)
- [x] Create `Monster.cs` (abstract)
- [x] Create `Townsperson.cs` (abstract)
- [x] Create `Interfaces/IEntity.cs` (clean — no Fly, LSP fix)
- [x] Create `Interfaces/IFlyable.cs`
- [x] Create `Interfaces/IShootable.cs`
- [x] Create `Interfaces/ISwimmable.cs`
- [x] Create `Interfaces/IDefendable.cs`
- [x] Create `Interfaces/ICommand.cs` (stretch)
- [x] Create `Ghost.cs` (IEntity, IFlyable)
- [x] Create `Goblin.cs` (IEntity)
- [x] Create `Troll.cs` (IEntity, ISwimmable)
- [x] Create `Models/Classes/Archer.cs` (IEntity, IShootable)
- [x] Create `Models/Classes/Fighter.cs` (IEntity, IDefendable)
- [x] Create `Models/Classes/Wizard.cs` (IEntity, IFlyable)
- [x] Create `Models/Classes/Rogue.cs` (IEntity)
- [x] Create `Models/Classes/Cleric.cs` (IEntity, IDefendable)
- [x] Create `Models/Classes/Knight.cs` (IEntity, IDefendable)
- [x] Create `Models/Classes/Blacksmith.cs` (IEntity, IDefendable)
- [x] Create `Models/Classes/Ranger.cs` (IEntity, IShootable, ISwimmable)
- [x] Create `Races/Human.cs` (no innate interfaces)
- [x] Create `Races/Elf.cs` (IFlyable)
- [x] Create `Races/Dwarf.cs` (IDefendable)
- [x] Create `Races/Orc.cs` (IEntity)
- [x] Create `Races/Halfling.cs` (ISwimmable)
- [x] Create `Services/Commands/` (Attack, Defend, Fly, Shoot, Swim)
- [x] Create `Services/GameEngine.cs`
- [x] Update `Program.cs` (namespaces + GameEngine wiring)
- [x] Update all existing files (namespaces only)

---

## ?? Known Gotchas

### Making `Character` Abstract + CsvHelper
CsvHelper needs a **concrete, instantiable** class when reading records.
If `Character` becomes abstract, `CsvFileHandler.ReadAll()` will break.

**Options:**
- Keep `Character` concrete for W5 ? (current approach)
- Introduce a `CharacterRecord` DTO for CsvHelper, map manually to Player/Npc (future)

### Race + Class Interface Stacking
A character has both a Race and a Class. Both can grant interfaces.
An Orc (IEntity) who is also a Fighter (IEntity, IDefendable) ends up with:
IEntity + IDefendable — C# handles duplicate interface declarations cleanly,
only one `Attack()` and one `Defend()` implementation needed.

### `Models/Classes/Archer` and the Ranger overlap
Both Archer and Ranger implement IShootable. That's fine — ISP allows multiple
classes to implement the same interface. They just have different Attack() flavors.

---

## Open Questions

- When does `Player` get race + class selection? ? Future character builder (W7+)
- `CharacterClass` abstract base with `ApplyClassBonuses(Character c)` — what week?
- Natural features (Tree, Rock, Water) — `WorldObject` or separate `NaturalFeature` base?
  ? `WorldObject` for now, revisit if the distinction adds value.
- `Door` inside a `Structure` vs standalone `WorldObject` ? same class, different context.
  A `Structure` holds a `List<WorldObject>` that includes `Door` instances.
- Should `Biome` hold a list of *allowed* `WorldObject` types, or instances?
  ? Type metadata only. Instances are owned by the world map (future).
- Bag/pouch as both Item and Container — where does this live?
  ? Probably `Item` with a `List<Item> Contents` property (future).
