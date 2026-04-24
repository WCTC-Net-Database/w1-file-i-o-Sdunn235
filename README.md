# Week 12: Inventory & Equipment + Advanced LINQ — Implementation

> **Student:** Shawn Dunn
> **Submitted:** 2026-04-23
> **Database:** `w9_efcore_SDunn` on `bitsql.wctc.edu`
> **Migration:** `W12_InventoryAndSeed`
> **Solution:** `w12-efcore-adv.sln` (continues the W11 codebase, renamed for this module)

---

## Overview

This submission completes the Week 12 assignment (Inventory & Equipment via Container TPH + advanced LINQ on items) and carries forward the LucentForge entity foundation laid down in Week 11.

The assignment was an **additive** layer on top of the W11 schema:

- A `Container` TPH was introduced with two subclasses (`Inventory`, `Equipment`) and an `IItemContainer` interface.
- `Item.ContainerId` became the single foreign key that moves an item between containers — picking up, dropping, equipping, and unequipping are all just FK updates.
- `Player` gained instance methods (`PickUp`, `Drop`, `Equip`, `Unequip`, `UseItem`) that read from `Character.Inventory` and `Character.Equipment`.
- A SQL-script seed migration seeds 10 Races, a reference Player (Elara the Bold), her stats/resources, her inventory + equipment containers, a starter kit, and one `EquipmentSlot` row per `SlotType`.

The graded LINQ tasks (Strongest Weapon, Total Value + GroupBy breakdown) are wired into a new **Inventory Management** submenu gated on the active character being a `Player`.

---

## Learning Objectives — Status

- [x] Model a polymorphic `Item` hierarchy using TPH (carried from W11: Weapon, Armor, Consumable)
- [x] Model a polymorphic `Container` hierarchy using TPH (Inventory, Equipment)
- [x] Understand the difference between item **instances** and item **types** (Container → Items is 1:many on `Item.ContainerId`)
- [x] Use advanced LINQ (`Where`, `GroupBy`, `OrderBy`, `OfType<T>`, `Sum`, `FirstOrDefault`) on in-memory collections
- [x] Implement inventory operations: pick up, equip, unequip, use, drop
- [x] Move items between containers by updating a single foreign key (`Item.ContainerId`)
- [x] Apply a seed data migration that runs a `.sql` script
- [x] **Graded Task A:** Strongest Weapon (25 pts)
- [x] **Graded Task B:** Total Value + GroupBy breakdown (25 pts)

---

## Design Deviations (Justified)

| Template Approach | My Implementation | Reason |
|-------------------|-------------------|--------|
| `KeyItem` as its own `Item` TPH subclass | `Item.IsKeyItem` bool flag | W11 committed `IsKeyItem` as a bool on `Item`. Converting to a subclass would force re-keying the Items table and offers no structural advantage — key items behave like other items, they just can't be sold or dropped. The W11 decision stands. |
| `Equipment` container holds items directly | `Equipment` container **owns `EquipmentSlot` rows**, which then reference items | W11 shipped slot-based equipping (MainHand / OffHand / Head / Chest / Legs / Feet / Hands). Ripping that out would reset W11 work for no benefit. `Equipment.Slots` replaces the flat item collection on the Equipment side and still satisfies the `IItemContainer` contract via the items-through-slots relationship. |
| Container TPH replaces `EquipmentSlot` | **Additive:** Container TPH added; `EquipmentSlot` kept, now owned by `Equipment` | Keeps W11 investment; sets up W13 chests and W14 rooms-as-containers as siblings of Inventory/Equipment without further reshuffling. |
| Intermediate abstract `Equipment` class under `Item` (W11) | Renamed to `DurableItem` in W12 | Avoid naming collision with the new `Containers.Equipment` class. No DB change — the W11 class was abstract and never a discriminator value. |

---

## What's New in W12

### New files

| Path | Purpose |
|------|---------|
| `ConsoleRpgEntities/Models/Containers/IItemContainer.cs` | Interface — Id, Name, Items, AddItem, RemoveItem |
| `ConsoleRpgEntities/Models/Containers/Container.cs` | Abstract TPH base with `ItemsCollection` + `AddItem`/`RemoveItem` |
| `ConsoleRpgEntities/Models/Containers/Inventory.cs` | `: Container` — `OwnerCharacterId`, `MaxWeight`, `CurrentWeight`, `CanFit` |
| `ConsoleRpgEntities/Models/Containers/Equipment.cs` | `: Container` — `OwnerCharacterId`, `Slots` (EquipmentSlot collection) |
| `ConsoleRpgEntities/Helpers/MigrationHelper.cs` | Reads SQL scripts from `Migrations/Scripts/` at migration-run time |
| `ConsoleRpgEntities/Migrations/BaseMigration.cs` | Abstract `Migration` subclass that exposes `RunSqlScript` |
| `ConsoleRpgEntities/Migrations/Scripts/W12_SeedInventoryData.sql` | Idempotent seed script (Races, Elara, containers, starter items, slots) |
| `ConsoleRpgEntities/Migrations/Scripts/W12_SeedInventoryData.rollback.sql` | Tear-down script used by the migration's `Down()` |
| `Docs/ASSIGNMENT_README.md` | Archived copy of the teacher's W12 template README |

### Modified files

| Path | Change |
|------|--------|
| `ConsoleRpgEntities/Models/Items/Item.cs` | + `ContainerId` (nullable int), + `Container` nav |
| `ConsoleRpgEntities/Models/Items/DurableItem.cs` | Renamed from `Equipment.cs` (abstract, no DB change) |
| `ConsoleRpgEntities/Models/Items/Weapon.cs`, `Armor.cs` | Inherit `DurableItem` instead of `Equipment` |
| `ConsoleRpgEntities/Models/EquipmentSlot.cs` | + `EquipmentContainerId` + `EquipmentContainer` nav |
| `ConsoleRpgEntities/Models/Character.cs` | + `Inventory?`, `Equipment?` navs; + `TypeName` helper (un-proxies lazy types) |
| `ConsoleRpgEntities/Models/Player.cs` | + `PickUp`, `Drop`, `Equip`, `Unequip`, `UseItem`, `PickSlotFor`, `BodySlotToSlotType` |
| `ConsoleRpgEntities/Data/GameContext.cs` | + `Containers` DbSet, Container TPH, 1:1 Character↔Inventory, 1:1 Character↔Equipment, 1:many Container→Items, 1:many Equipment→EquipmentSlots |
| `ConsoleRpgEntities/Data/IContext.cs` | + `IEnumerable<Container> Containers` |
| `ConsoleRpgEntities/ConsoleRpgEntities.csproj` | `Migrations/Scripts/*.sql` copied to output |
| `ConsoleRpg/Services/GameEngine.cs` | `FindCharacter` → `SelectCharacter`, `_activeCharacter`, `TypeName` fix, Rooms list characters, `InventoryMenu` + graded LINQ |
| `ConsoleRpg/UI/ConsoleGameUi.cs` + `IGameUi.cs` | Active-character header line; new "Inventory Management" menu option |
| `ConsoleRpg/Program.cs` | Rewired menu for `SelectCharacter` + `InventoryMenu` |
| `w10-efcore-tph.sln` → `w12-efcore-adv.sln` | Solution file renamed to match module |

---

## Relationships Configured

| Principal | Dependent | Cardinality | FK | On Delete |
|-----------|-----------|-------------|----|-----------|
| `Character` | `Inventory` | 1 : 0..1 | `Inventory.OwnerCharacterId` | `ClientCascade` |
| `Character` | `Equipment` | 1 : 0..1 | `Equipment.OwnerCharacterId` | `ClientCascade` |
| `Container` | `Item` | 1 : many | `Item.ContainerId` (nullable) | `SetNull` |
| `Equipment` | `EquipmentSlot` | 1 : many | `EquipmentSlot.EquipmentContainerId` | `Cascade` |

> Why `ClientCascade` on the Character↔Container FKs: SQL Server rejects two cascade paths from the same principal table. EF Core handles the cascade client-side when the Character is tracked, which is how the app accesses containers anyway (via lazy-loaded navs).

### Empty/null tolerance (design principle)

Every container relationship tolerates both empty *and* null:

- **Empty is normal.** `Container.ItemsCollection` starts as `new List<Item>()`; a Container with zero items is valid.
- **Character-side is nullable.** `Character.Inventory?` and `Character.Equipment?` — a character can exist before being given a backpack or gear.
- **Item-side is nullable.** `Item.ContainerId` is `int?`. Items can float (orphaned during a move, or dropped in an unpopulated room before W14).
- **Future chests (W13) and rooms-as-containers (W14)** will be their own Container subclasses — no `OwnerCharacterId` at all, so "no owner" is the shape of those subclasses.

---

## Graded LINQ Tasks

### Task A — Strongest Weapon

```csharp
var strongest = _activePlayer.Inventory.ItemsCollection
    .OfType<Weapon>()
    .OrderByDescending(w => w.AttackPower)
    .FirstOrDefault();
```

With Elara's seeded kit (Rusty Sword AP=7, Oak Bow AP=9), this returns **Oak Bow — Attack 9 (Bow)**.

### Task B — Total Value + GroupBy breakdown

```csharp
int total = items.Sum(i => i.Value);
var breakdown = items
    .GroupBy(i => i.TypeNameForItem())
    .Select(g => new { Type = g.Key, Gold = g.Sum(i => i.Value), Count = g.Count() })
    .OrderByDescending(x => x.Gold)
    .ToList();
```

Output format:

```
--- Inventory Value ---
  Total: 150g across 7 items
  By type:
    Armor         2 items      60g
    Weapon        2 items      45g
    Consumable    3 items      45g
```

---

## Migration: `W12_InventoryAndSeed`

Single migration handles both schema *and* seed:

1. **Schema (`Up`):**
   - Adds `Items.ContainerId` (nullable FK → Containers, OnDelete SetNull)
   - Adds `EquipmentSlots.EquipmentContainerId` (nullable FK → Containers, OnDelete Cascade)
   - Creates `Containers` table (Id, Name, ContainerType discriminator, OwnerCharacterId, Inventory_OwnerCharacterId, MaxWeight)
   - Adds indexes + FK constraints

2. **Seed (`Up` — after schema):** calls `RunSqlScript(migrationBuilder, "W12_SeedInventoryData.sql")` which:
   - Inserts 10 Races (3 Playable: Human/Elf/Dwarf, 7 Monster: Goblin/Orc/Troll/Mimic/Slime/Chimera/Kobold)
   - Inserts Elara the Bold (Player, Human, Level 1) + her Stats + Resources
   - Inserts her `Inventory` (MaxWeight 100) and `Equipment` containers
   - Inserts starter items: Rusty Sword, Oak Bow, Leather Helm, Leather Tunic, Healing Potion, Stamina Draught, Old Brass Key (IsKeyItem=1)
   - Inserts one `EquipmentSlot` per `SlotType` (MainHand, OffHand, Head, Chest, Legs, Feet, Hands), all empty

All inserts are guarded with `NOT EXISTS` checks against stable Names, so the migration is idempotent — safe to re-run against a shared DB.

3. **`Down`** runs `W12_SeedInventoryData.rollback.sql` first (removes Elara's rows in reverse FK order), then drops the schema.

---

## Running the Game

```
14. Inventory Management       ← new W12 submenu
  1. List items (with weight)
  2. Search by name
  3. Group by type
  4. Sort items
  5. Equip item from inventory
  6. Unequip item
  7. Use consumable
  8. Strongest weapon (graded)
  9. Total value + breakdown (graded)
  0. Back to main menu
```

Flow:
1. From the main menu, choose **2. Select Character** and enter `Elara` (the active-character header then reads `[Active: Elara the Bold (Player)]`).
2. Choose **14. Inventory Management** — the submenu only enables when the active character is a `Player` with an `Inventory` container.
3. All options operate on `_activePlayer.Inventory.ItemsCollection` in memory; **8** and **9** are the graded LINQ tasks.

---

## Rubric Self-Assessment

| Criterion | Weight | Status | Notes |
|-----------|--------|--------|-------|
| Container TPH (Inventory + Equipment) with `Item.ContainerId` FK | required | ✅ | `Container` abstract + 2 subclasses; `Items.ContainerId` nullable FK with SetNull |
| Player instance methods (PickUp/Drop/Equip/Unequip/UseItem) | required | ✅ | All five present; operate on `Inventory`/`Equipment` navs |
| Inventory sub-menu with LINQ ops (Where/GroupBy/OrderBy/OfType/Sum) | required | ✅ | 9-option submenu; every required LINQ operator used |
| Graded Task A: Strongest Weapon | 25 pts | ✅ | `OfType<Weapon>().OrderByDescending(w => w.AttackPower).FirstOrDefault()` |
| Graded Task B: Total Value + breakdown | 25 pts | ✅ | `Sum` + `GroupBy` + `Select` projection with ordered output |
| Seed migration via `.sql` script | required | ✅ | `W12_InventoryAndSeed` + `MigrationHelper` + `BaseMigration` + `Migrations/Scripts/*.sql` copied to output |
| Weight limit UX (stretch) | +10 pts | ✅ | `Inventory.CurrentWeight`/`CanFit`; inventory list shows `X / MaxWeight lbs` |
| **Target** |  | **100 + 10 stretch** |  |

---

## LucentForge Bible Alignment

The Container pattern introduced here is the foundation for:

- **W13** — Chests as a third `Container` subclass (no `OwnerCharacterId`; room-attached), monster loot drops become chests that spawn on death.
- **W14** — Rooms-as-containers. `Room` will gain a `RoomContainer` child so dropped items in a room are actually in a container, not floating with `ContainerId = NULL`.
- **W15+** — Crafting stations and merchant stock as further Container subclasses.

By building Container TPH cleanly this week — additive to EquipmentSlot, with nullable owner FKs and a settled empty/null stance — the rest of the semester should be subclass extensions, not re-architecture.

---

## Known Issues / Follow-up

- The scaffolded Container table has both `OwnerCharacterId` and `Inventory_OwnerCharacterId` columns (EF Core disambiguated the same-named property on two TPH subclasses). Only one is populated per row, gated by discriminator. This is cosmetic, not functional.
- `DisplayCurrentRoom` and `MovePlayer` still pick "any Player" via `OfType<Player>()` when `_activeCharacter` isn't set, then prompt. A cleaner follow-up would force selection up front.
- Inventory menu does not yet support "pick up from room" or "drop to room" — W14 territory, when rooms become containers.

---

## Verification Steps

1. `dotnet build w12-efcore-adv.sln` — 0 warnings, 0 errors
2. `dotnet ef database update --project ConsoleRpgEntities --startup-project ConsoleRpg` — migration applies cleanly
3. SSMS: `SELECT COUNT(*) FROM Races` → 10; `SELECT * FROM Containers` → 2 rows for Elara; `SELECT * FROM Items WHERE ContainerId IS NOT NULL` → starter kit
4. App walkthrough: Select Elara → Inventory Management → all 9 options produce expected output
