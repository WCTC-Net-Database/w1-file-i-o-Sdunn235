# Week 11: Equipment System & Room Navigation — Implementation

> **Student:** Shawn Dunn
> **Submitted:** 2026-04-17
> **Database:** `w9_efcore_SDunn` on `bitsql.wctc.edu`
> **Migration:** `W11_EquipmentRoomNavLucentForge`

---

## Overview

This submission completes the Week 11 assignment (Equipment + Room Navigation) and its stretch goal (Item TPH). It also extends the codebase into the entity foundation for my capstone RPG, **LucentForge** — adding Stats, Resources, Races, Skills, Magic, and stat-scaled Abilities.

All Week 11 rubric criteria are met. The shape of some entities differs from the template (see "Design Deviations" below), but the underlying EF Core relationship patterns (self-referencing FKs, one-to-one, one-to-many, TPH, many-to-many with join entities) are all exercised.

---

## Learning Objectives — Status

- [x] Create an Equipment entity with Weapon and Armor properties
- [x] Create a Room entity with directional navigation (N/S/E/W + Up/Down via Door entity)
- [x] Configure self-referencing relationships (Door → Room Source + Destination)
- [x] Configure one-to-one relationships (Character ↔ Stats, Character ↔ Resources)
- [x] Configure one-to-many relationships (Room → Doors, Character → EquipmentSlots)
- [x] Configure many-to-many with explicit join (Character ↔ Skill via CharacterSkill)
- [x] Implement basic room navigation in the game
- [x] Generate and apply migration for new entities
- [x] **Stretch: Item TPH** (Item → Equipment → Weapon/Armor, Item → Consumable)

---

## Design Deviations (Approved / Justified)

| Template Approach | My Implementation | Reason |
|-------------------|-------------------|--------|
| `Room.NorthRoomId / SouthRoomId / EastRoomId / WestRoomId` (4 self-FKs) | `Door` entity with `SourceRoomId` + `DestinationRoomId` + `Direction` enum + `IsLocked` | Richer: a Door is a first-class entity that can be locked, named, and described. Still exercises self-referencing FKs (Door → Room twice). Supports 6 directions (N/S/E/W/Up/Down) instead of 4. |
| `Equipment` holds a single `WeaponId + ArmorId` | `EquipmentSlot` entity (one row per slot) with `SlotType` enum | A character equips multiple armor pieces (head, chest, legs, feet, hands) plus main/off-hand weapons. A single Equipment row can't express that. |
| `Item.Type` string ("Weapon"/"Armor") | TPH: `Item` abstract → `Equipment` abstract → `Weapon`, `Armor`; `Item` → `Consumable` | Stretch goal, plus adds Consumable for potions/food. |
| `Player` and `Monster` each have `RoomId` | `Character.RoomId` (nullable) on the TPH base | Placement is a Character-level concern. Player/NPC/Animal all inherit it. |
| Goblin as Character subtype | Goblin as a `MonsterRace`; characters use `Character → Race` FK | Race is data, not a subclass. Same Goblin race can be applied to many NPCs. Migration converts old Goblin rows → NPC discriminator. |

---

## What I Built

### Entities (19 total)

**Core character system:**
- `Character` (TPH base) — RoomId (nullable), RaceId, navs to Stats/Resources/EquipmentSlots/Skills/Abilities/Magics. Derivation helpers: `DeriveMaxHp()`, `DeriveMaxSp()`, `DeriveMaxBp()`, `DeriveMaxBytePool()`, `GetTotalAttack()`, `GetTotalDefense()`.
- `Player`, `Npc`, `Animal` (TPH subtypes)
- `Stats` — 7 attributes (Physique, Reflexes, Constitution, Intellect, Intuition, Linguistic, Luck). 1:1 with Character.
- `Resources` — Hp/MaxHp, Sp/MaxSp, Bp/MaxBp, BytePool/MaxBytePool. 1:1 with Character.

**Race system (TPH):**
- `Race` (abstract) — Id, Name, Description
- `PlayableRace`, `MonsterRace`, `AnimalRace`

**World:**
- `Room` — Name, Description, navs to Doors and Characters
- `Door` — SourceRoomId, DestinationRoomId, Direction, IsLocked, Name, Description

**Items (TPH):**
- `Item` (abstract) — Name, Description, Value, Weight, IsKeyItem
- `Equipment` (abstract) — adds Durability
- `Weapon` — AttackPower, WeaponType
- `Armor` — DefenseRating, WeightClass, BodySlot
- `Consumable` — Effect, Potency

**Equipment slots:**
- `EquipmentSlot` — CharacterId, SlotType, nullable EquippedItemId

**Skills & abilities:**
- `Skill` — Name, Description, PrimaryAttribute (CoreAttribute), SecondaryAttribute? (CoreAttribute?)
- `CharacterSkill` (join) — composite key (CharacterId, SkillId), Proficiency (0–100)
- `Ability` — Power, StaminaCost, Kind (AbilityKind), PrimaryStat (CoreAttribute). M:M with Character.
- `Magic` — Power, BpCost, BytePoolCost, Element, Kind (MagicKind), PrimaryStat. M:M with Character.

### Enums (9 total)

`CoreAttribute`, `Direction`, `SlotType`, `ArmorWeight`, `BodySlot`, `WeaponType`, `Element`, `AbilityKind`, `MagicKind`

---

## Relationships Configured

| Pattern | Example |
|---------|---------|
| TPH (discriminator) | `Character` (Player/NPC/Animal), `Race` (Playable/Monster/Animal), `Item` (Weapon/Armor/Consumable) |
| One-to-One | `Character ↔ Stats`, `Character ↔ Resources` |
| One-to-Many | `Room → Doors`, `Character → EquipmentSlots`, `Race → Characters` |
| Many-to-Many (implicit) | `Character ↔ Ability`, `Character ↔ Magic` |
| Many-to-Many (explicit join) | `Character ↔ Skill` via `CharacterSkill` (carries Proficiency) |
| Self-referencing | `Door → Room` twice (Source + Destination) |
| Nullable FK with SetNull delete | `Character.RoomId` |

---

## LucentForge Bible Alignment

This is the data layer for my capstone game. Three bible tie-ins worth calling out:

1. **Stats drive everything.** `Character.DeriveMaxHp()` = `50 + Constitution * 5`. Similar derivations for SP, BP, and BytePool. Resources aren't arbitrary — they derive from stats.
2. **Bits/Bytes magic system.** `Magic.BpCost` (raw instinctive magic, scales off Intuition) vs `Magic.BytePoolCost` (structured deliberate magic, scales off Intellect). Both on the same entity; `PrimaryStat` encodes which applies.
3. **CoreAttribute as the scaling bridge.** `Ability.PrimaryStat` and `Magic.PrimaryStat` both reference `CoreAttribute`, so future combat resolution can compute effective power as `Stats[PrimaryStat] + Power`.

---

## Migration Notes

Migration `W11_EquipmentRoomNavLucentForge` was **hand-edited** after scaffolding to safely convert existing data:

- **Dropped** the old `Sneakiness` column on Characters (instead of the scaffolder's rename-to-RaceId, which would have moved junk data into a real FK column).
- **Added** `RaceId` as a fresh nullable FK.
- **Converted** existing `Goblin` TPH discriminator rows to `NPC` via raw SQL so legacy records survive the refactor.

---

## Running the Game

Menu options (Program.cs):

| # | Option |
|---|--------|
| 1 | Display Characters |
| 2 | Find Character |
| 3 | Add Character |
| 4 | Level Up Character |
| 5 | Display Character Detail |
| 6 | Display Rooms |
| 7 | Add Room |
| 8 | Add Door |
| 9 | Display Current Room |
| 10 | Move Player |
| 11 | Display Equipment |
| 12 | Equip Item |
| 13 | Add Item |
| 0 | Exit |

---

## Project Structure

```
w11-efcore-equipment/
│
├── ConsoleRpg/                                 # UI & service layer
│   ├── Program.cs
│   ├── Startup.cs
│   ├── Services/GameEngine.cs
│   └── UI/
│       ├── IGameUi.cs
│       └── ConsoleGameUi.cs
│
└── ConsoleRpgEntities/                         # Data & entities
    ├── Data/
    │   ├── GameContext.cs                      # All DbSets + OnModelCreating
    │   └── IContext.cs
    ├── Migrations/
    │   └── 20260417094734_W11_EquipmentRoomNavLucentForge.cs
    └── Models/
        ├── Character.cs  Player.cs  Npc.cs  Animal.cs
        ├── Stats.cs  Resources.cs
        ├── Room.cs  Door.cs  EquipmentSlot.cs
        ├── Abilities/Ability.cs
        ├── Enums/ (9 enums)
        ├── Items/Item.cs  Equipment.cs  Weapon.cs  Armor.cs  Consumable.cs
        ├── Magic/Magic.cs
        ├── Races/Race.cs  PlayableRace.cs  MonsterRace.cs  AnimalRace.cs
        └── Skills/Skill.cs  CharacterSkill.cs
```

---

## Rubric Self-Assessment

| Criteria | Points | Status |
|----------|--------|--------|
| Equipment Class | 15 | Complete — `EquipmentSlot` per slot, richer than template |
| Item Class | 15 | Complete — TPH hierarchy |
| Room Class | 15 | Complete — with `Door` entity for richer navigation |
| Player/Monster Integration | 15 | Complete — `Character.RoomId` on TPH base |
| GameContext Setup | 15 | Complete — 3 TPH hierarchies, all relationships configured |
| Room Navigation | 15 | Complete — `MovePlayer()`, `DisplayCurrentRoom()` |
| Code Quality | 10 | Clean, lazy-loaded, SOLID |
| **Total** | **100** | |
| **Stretch: Item TPH** | **+10** | Complete — Item → Equipment → Weapon/Armor, Item → Consumable |

---

## Known Issues / Follow-up

Observed during testing; tracked for next iteration:

1. `GetType().Name` returns `"PlayerProxy"` for lazy-loaded entities (should use `BaseType`)
2. `Display Rooms` doesn't list characters currently in each room
3. No "active character" concept — `Find Character` should select, not just display
4. Character type selection (Player/NPC/Animal) could be clearer in the UI
5. No inventory for Consumables; no slot validation (can equip a potion to feet)
6. `Consumable.Effect` / `Potency` have no help text explaining expected values
7. Race data not seeded (Human, Elf, Dwarf, Goblin, Orc, Troll, Mimic, Slime, Chimera, Kobold)
