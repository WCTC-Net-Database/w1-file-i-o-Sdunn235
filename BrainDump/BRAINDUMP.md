# ConsoleRPG - Brain Dump & Planning
> Week 6: Dependency Inversion Principle (DIP)
> Last Updated: 2026-03-06

---

## Core Design Philosophy

> "Anything a Player can do, an NPC can do too."

All entities - Player and NPC alike - derive from `CharacterBase`.
Capabilities (flying, shooting, swimming, defending) are granted by interfaces, not by class position.
`GameEngine` works against `IEntity` exclusively - it never cares if something is a `Player` or `Monster`.
**Races** define *what you are*. **Classes** define *your role*. **Interfaces** define *what you can do*.

---

## W6 Key Changes from W5

| Change | Description |
|--------|-------------|
| `CharacterBase` | New abstract base with `PerformSpecialAction()` contract |
| `Character` | Now extends `CharacterBase` (concrete, for CsvHelper only) |
| `CharacterMap` | Moved from `Models/Characters/` to `Services/` (removes CsvHelper from Models) |
| `IFileHandler` | Updated to use `CharacterBase` instead of `Character` |
| `FileHandlerFactory` | Factory class so `Program.cs` depends on `IFileHandler` abstraction (DIP) |
| `GameEngine` | Calls `PerformSpecialAction()` via `CharacterBase` abstraction |
| New classes | `Necromancer` (raises undead) and `Paladin` (lays on hands) - stretch goal |
| Namespace | Renamed from `W5SolidLsp` to `W6DependencyInversion` |

---

## Current File Structure (W6)

```
ConsoleRPG/
+-- Program.cs
+-- BrainDump/
|   +-- BRAINDUMP.md                                    <- this file
+-- Services/
|   +-- GameEngine.cs                                   <- calls PerformSpecialAction() via CharacterBase (W6)
|   +-- CharacterUI.cs
|   +-- CharacterWriter.cs
|   +-- CharacterReader.cs
|   +-- MenuService.cs
|   +-- CsvFileHandler.cs
|   +-- JsonFileHandler.cs
|   +-- CharacterMap.cs                                 <- MOVED HERE from Models/ (W6: removes CsvHelper from Models)
|   +-- FileHandlerFactory.cs                           <- NEW W6: factory for DIP
|   +-- Commands/
|       +-- AttackCommand.cs                            <- W5 Stretch: Command Pattern
|       +-- DefendCommand.cs                            <- W5 Stretch
|       +-- FlyCommand.cs                               <- W5 Stretch
|       +-- ShootCommand.cs                             <- W5 Stretch
|       +-- SwimCommand.cs                              <- W5 Stretch
+-- Models/
|   +-- Characters/
|   |   +-- CharacterBase.cs                            <- NEW W6: abstract base with PerformSpecialAction()
|   |   +-- Character.cs                                <- concrete CSV/JSON record (extends CharacterBase)
|   |   +-- Players/
|   |   |   +-- Player.cs
|   |   +-- Npcs/
|   |       +-- Npc.cs                                  <- abstract, has Faction
|   |       +-- Monsters/
|   |       |   +-- Monster.cs                          <- abstract, Faction = "Hostile"
|   |       |   +-- Ghost.cs                            <- IEntity, IFlyable
|   |       |   +-- Goblin.cs                           <- IEntity
|   |       |   +-- Troll.cs                            <- IEntity, ISwimmable
|   |       +-- Townspeople/
|   |           +-- Townsperson.cs                      <- abstract, Faction = "Neutral"
|   |           +-- Races/
|   |               +-- Human.cs
|   |               +-- Elf.cs                          <- IFlyable (racial trait)
|   |               +-- Dwarf.cs                        <- IDefendable (racial trait)
|   |               +-- Orc.cs                          <- IEntity (racial trait)
|   |               +-- Halfling.cs                     <- ISwimmable (racial trait)
|   +-- Classes/                                        <- RPG class templates (not C# classes!)
|       +-- Archer.cs                                   <- IEntity, IShootable
|       +-- Fighter.cs                                  <- IEntity, IDefendable
|       +-- Wizard.cs                                   <- IEntity, IFlyable
|       +-- Rogue.cs                                    <- IEntity
|       +-- Cleric.cs                                   <- IEntity, IDefendable
|       +-- Knight.cs                                   <- IEntity, IDefendable
|       +-- Blacksmith.cs                               <- IEntity, IDefendable
|       +-- Ranger.cs                                   <- IEntity, IShootable, ISwimmable
|       +-- Necromancer.cs                              <- NEW W6 Stretch: IEntity, IFlyable
|       +-- Paladin.cs                                  <- NEW W6 Stretch: IEntity, IDefendable
+-- Interfaces/
|   +-- IFileHandler.cs                                 <- W4/W5/W6: uses CharacterBase (DIP)
|   +-- IEntity.cs                                      <- W5: clean (no Fly - LSP fix)
|   +-- IFlyable.cs                                     <- W5
|   +-- IShootable.cs                                   <- W5
|   +-- ISwimmable.cs                                   <- W5
|   +-- IDefendable.cs                                  <- W5
|   +-- ICommand.cs                                     <- W5 Stretch
+-- Input/
    +-- input.csv
    +-- input.json
```

---

## Character Hierarchy (W6)

```
CharacterBase (abstract - W6: PerformSpecialAction() required)
+   string Name, Class, Equipment
+   int Level, Hp
+   abstract void PerformSpecialAction()
|
+-- Character (concrete - CsvHelper/JSON record only)
    |
    +-- Player (Players/Player.cs)
    |       User-controlled
    |
    +-- Npc (abstract - Npcs/Npc.cs)
        +   string Faction
        |
        +-- Monster (abstract - Npcs/Monsters/Monster.cs)
        |   Faction = "Hostile"
        |   +-- Ghost     -> IEntity, IFlyable
        |   +-- Goblin    -> IEntity
        |   +-- Troll     -> IEntity, ISwimmable
        |
        +-- Townsperson (abstract - Npcs/Townspeople/)
            Faction = "Neutral" by default
            +-- Races/
                +-- Human
                +-- Elf       -> IFlyable
                +-- Dwarf     -> IDefendable
                +-- Orc       -> IEntity
                +-- Halfling  -> ISwimmable
```

---

## SOLID Compliance Summary (W6)

| Principle | How It's Applied |
|-----------|-----------------|
| **S**RP | `GameEngine` runs loop; `CharacterUI` handles display; `FileHandlerFactory` handles creation |
| **O**CP | New entity types slot in; new file formats need only a new factory case |
| **L**SP | Every subtype substitutes safely - CharacterBase -> Character -> Npc -> Monster is a safe chain |
| **I**SP | 6 small behavior interfaces; no class implements what it can't do |
| **D**IP | `GameEngine` depends on `IEntity`; `Program.cs` depends on `IFileHandler` via `FileHandlerFactory` |

---

## W6 Build Order (Completed)

- [x] Update namespace W5SolidLsp -> W6DependencyInversion across all files + csproj
- [x] Create `CharacterBase.cs` (abstract, PerformSpecialAction() required)
- [x] Update `Character.cs` to extend CharacterBase
- [x] Move `CharacterMap.cs` from Models/ to Services/ (removes CsvHelper from Models)
- [x] Update `IFileHandler.cs` to use CharacterBase
- [x] Update `CsvFileHandler.cs` - CharacterBase interface, Character internally
- [x] Update `JsonFileHandler.cs` - CharacterBase interface, Character internally
- [x] Update `CharacterUI.cs`, `CharacterReader.cs`, `CharacterWriter.cs`
- [x] Add `PerformSpecialAction()` to all concrete classes
- [x] Create `FileHandlerFactory.cs` for DIP
- [x] Update `Program.cs` (DIP via factory, W6 references)
- [x] Update `GameEngine.cs` (calls PerformSpecialAction via CharacterBase)
- [x] Create `Necromancer.cs` (stretch: IEntity, IFlyable, raises undead)
- [x] Create `Paladin.cs` (stretch: IEntity, IDefendable, lays on hands)
