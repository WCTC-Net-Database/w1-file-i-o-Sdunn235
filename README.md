# Week 14: Rooms & Doors

> **Template Purpose:** This template represents a working solution through Week 13. Use YOUR repo if you're caught up. Use this as a fresh start if needed.

---

## Overview

This is the week where everything clicks into place. You'll:

1. **Add `Room` as yet another `Container` subclass** — items on the floor are now just items in a container whose type happens to be "Room"
2. **Add `Door` as a separate entity** that connects two rooms and implements the **same `ILockable` interface** you built for chests in Week 13
3. **Watch `Player.TryUnlock` work on doors** — without modification — because the method now takes an `ILockable`, not a `Chest`

This is the payoff week for the SOLID architecture you've been building all semester. When your W13 unlock code opens a W14 door, that's the Liskov Substitution Principle clicking into place. The same function, the same algorithm, the same lockpicking feel — applied to a completely different entity with zero duplication.

> **Light week on scope, heavy on concepts.** W14 is intentionally compact so we can spend most of class on final-project Q&A. The assignment itself is small; the ideas it demonstrates are big. Come ready to ask questions about your final project!

## Learning Objectives

By completing this assignment, you will:
- [ ] Add a fifth subclass to the `Container` TPH hierarchy without modifying existing code
- [ ] Create a new entity (`Door`) that implements an existing interface (`ILockable`)
- [ ] Refactor a method from concrete-type-parameter to interface-parameter (LSP in action)
- [ ] Model self-referencing foreign keys for room navigation
- [ ] Model a "join-like" entity (`Door`) with two FKs to the same table
- [ ] Use LINQ to query the world (find exits, visible doors, items on the floor)
- [ ] Persist player and monster locations across sessions

## Prerequisites

- [ ] Completed Week 13 (or using this template)
- [ ] Understanding of TPH extension and `ILockable` from W13
- [ ] Basic LINQ + `OfType<T>()`

---

## Design Deviations (Justified)

This section logs deliberate engineering decisions made on the rolling
codebase as W14 work proceeds. Following the same format as Module 13's
README — each row is a place where I chose something different from the
default/template approach and the reason it was the better call.

### Pre-W14 polish — Consumable Effect refactor (Phase B)

A pre-W14 cleanup pass landed before the schema work. The W13 codebase
shipped with `Consumable.Effect` as a free-text `string`, and a quiet bug:
unrecognized effect values silently consumed the item without producing
any change. Phase B addressed both issues.

| Old approach (W12/W13) | New approach (Phase B) | Reason |
|---|---|---|
| `Consumable.Effect` is `string`. Free text. Lowercased and switched on inside `Character.UseItem`. | `Consumable.Effect` is the typed `ConsumableEffect` enum (`None / Heal / Stamina / BitPool / BytePool`). | Eliminates magic strings (SOLID — the unlock algorithm depends on a stable abstraction, not a stringly-typed value). Makes the create-item menu a numbered picker that auto-updates when a new enum value is added. |
| Unrecognized effect strings (typos, or rows where Effect was a *type discriminator* like `'keyitem'`/`'lockpick'`) fell through the switch — but the item was still consumed by the `Inventory.RemoveItem` call outside the switch. Silent no-op. | `UseItem` tracks an `applied` flag inside the switch; consumption only fires when a real effect was applied. `ConsumableEffect.None` rows are intentionally non-consuming. | Compile-time exhaustiveness + behavior fix. Iron Lockpicks (which were stored as `Effect = "lockpick"` Consumable rows) no longer "vanish" on accidental use-item attempts. |
| `Effect = "keyitem"` and `Effect = "lockpick"` overloaded the column with type-discriminator meaning on rows already discriminated as `Consumable`. | Phase B: those rows are migrated to `ConsumableEffect.None` so the column means *only* "what does this consumable do." Phase C will move them to a proper `KeyItem : Item` TPH subclass and drop `Item.IsKeyItem`. | One smell at a time. Phase B narrows `Effect`'s contract; Phase C removes the rows that don't belong here. The composability is the story — each commit removes one ambiguity without bleeding into the next. |
| Migration scaffolded as plain `AlterColumn(string -> int)`, which would fail on SQL Server (can't cast `'heal'` to `int`). | Hand-edited `W14_ConvertConsumableEffectToEnum` migration: temp `EffectNew` column with `DEFAULT 0` → `CASE LOWER(Effect)` backfill → drop legacy → rename. Reverse mapping in Down. | Same hand-edit pattern from W11's Goblin-discriminator migration. Default constraint dropped at end of Up to avoid a dangling `DF_Items_EffectNew` constraint name on a column called `Effect` (future-reader crisis prevention). |

**Verification SQL** (run against `w9_efcore_SDunn` after the migration applies):

```sql
SELECT Name, Effect, Potency, IsKeyItem
FROM Items
WHERE ItemType = 'Consumable'
ORDER BY Effect, Name;
```

Expected:
- `Healing Potion`, `Lesser Healing Draught`, `Antidote` → `Effect = 1` (Heal)
- `Stamina Draught`, `Gobbo's Stew` → `Effect = 2` (Stamina)
- `Elixir of the Wakeful` → `Effect = 4` (BytePool)
- `Old Brass Key`, `Dungeon Key`, `Iron Lockpick #1`, `Iron Lockpick #2` → `Effect = 0` (None — these are Phase C's KeyItem candidates)

### Pre-W14 polish — W12 unique-index filter fix (Phase B.1)

A latent bug in `W12_InventoryAndSeed` slept until Phase 1.5 (C0020) put
multiple Equipment rows in the database — Phase B's first end-to-end test
was the trigger, not the cause.

| Old approach (W12) | New approach (Phase B.1) | Reason |
|---|---|---|
| `IX_Containers_Inventory_OwnerCharacterId` had `column: "Inventory_OwnerCharacterId"` (correct) but `filter: "[OwnerCharacterId] IS NOT NULL"` (wrong column). The filter included every Equipment row, all of which have `Inventory_OwnerCharacterId = NULL`. With one Equipment row (Elara) the unique index tolerated a single NULL key; with two it crashed: *"Cannot insert duplicate key row ... duplicate key value is (NULL)."* | `W14_FixContainerInventoryIndexFilter` migration drops the broken index and recreates with `filter: "[Inventory_OwnerCharacterId] IS NOT NULL"`. Companion `IX_Containers_OwnerCharacterId` was already correct (column and filter agree); only the Inventory side needed repair. | EF's TPH disambiguation auto-generated two FK columns (`OwnerCharacterId` for Equipment, `Inventory_OwnerCharacterId` for Inventory). The filter clause must reference the index's own column, not its TPH sibling's. Fixed forward — the W14 Room subclass is naturally excluded from both indexes since Rooms have NULL on both columns. |

**Lesson captured to Claude memory:** "Test functional before committing."
Build-green + migration-applied is necessary but not sufficient — the
verification is exercising the affected code path end-to-end. C0022 was
committed without a `dotnet run` test; the latent W12 bug surfaced
moments later. Going forward, runtime-affecting commits get a manual
exercise pass before the commit lands.

### W14 implementation deviations
*(to be filled as W14 schema and graded LINQ work proceeds)*

---

## What's New This Week

| Concept | Description |
|---------|-------------|
| `Room` | New Container subclass — holds items on the floor |
| `Door` | New entity — connects two rooms, can be locked/trapped/secret |
| `Room.NorthRoomId`, `SouthRoomId`, ... | Self-referencing FKs for navigation |
| `Player.CurrentRoomId` / `Monster.CurrentRoomId` | Entity location persistence |
| `ILockable TryUnlock` | Refactored to accept any `ILockable`, not just chests |
| `Door.IsSecret` / `IsDiscovered` | New state that doesn't exist on chests |
| Single Door row per passage | One row per door regardless of which side the player approaches from |
| **Spectre.Console (intro)** | A taste of styled console output — used heavily in the Week 15 final |

---

## Spectre.Console: A Small Taste

Take a look at `Services/GameEngine.cs` at the `PrintRoomHeader` method. You'll see your first **Spectre.Console** Panel — a small styled box with the room name as a header and the player's HP as the body.

```csharp
var panel = new Panel($"[green]HP:[/] {_player.Health}")
{
    Header = new PanelHeader($"[yellow bold]{room.Name}[/]"),
    Border = BoxBorder.Rounded,
    Padding = new Padding(1, 0, 1, 0)
};
AnsiConsole.Write(panel);
```

Spectre.Console is a library for building rich console output: panels, tables, trees, progress bars, live updates, even ASCII maps. Week 14 just shows you `Panel` and the `[color]...[/]` markup — Week 15 uses it for a full split-panel exploration UI with a live world map.

**Don't worry about it yet.** The rest of this assignment uses plain `Console.WriteLine`. Spectre is introduced here so the jump to the final project in Week 15 doesn't feel sudden.

- [Spectre.Console docs](https://spectreconsole.net/)
- [Panel widget](https://spectreconsole.net/widgets/panel)

---

## The Big Idea: Room IS a Container

We've been building toward this since Week 12:

```csharp
public class Room : Container { ... }  // Room is a Container
```

That means dropping an item on the floor is no longer a special operation. It's the exact same operation as putting it in a backpack or a chest:

```csharp
// Drop item from backpack onto the floor of the current room:
item.ContainerId = currentRoom.Id;
_context.SaveChanges();
```

Pick up? Same thing in reverse:
```csharp
item.ContainerId = player.Inventory.Id;
```

**Every item in your game — in backpacks, equipment slots, chests, monster corpses, AND on the floors of rooms — lives in a single `Items` table with a single `ContainerId` foreign key.** One LINQ query can ask "where is this specific sword right now?" and the answer is just `item.Container.ContainerType`.

This is the culmination of the "items are instances, not types" principle from W12. You can now run:

```csharp
var allSwords = _context.Items.OfType<Weapon>()
    .Where(w => w.Name == "Iron Shortsword")
    .Select(w => new { w.Name, Location = w.Container })
    .ToList();
```

...and see every Iron Shortsword in the world along with where it currently lives. Backpacks, chests, monster loot, room floors — all unified under one model.

---

## The Big Payoff: Lockable Doors

In Week 13 your `Player.TryUnlock` looked like this:

```csharp
public bool TryUnlock(Chest chest, KeyItem key) { ... }
```

Perfect for chests. Useless for doors. So in Week 14 we made **one tiny change**:

```csharp
public bool TryUnlock(ILockable target, KeyItem key) { ... }
```

That's it. Swap `Chest` for `ILockable`, change every `chest.X` to `target.X`, done. And now:

```csharp
player.TryUnlock(chest, dungeonKey);  // Works (W13)
player.TryUnlock(door, cellarKey);    // ALSO WORKS (W14) - zero code duplication
```

This is the **Liskov Substitution Principle**: any implementation of `ILockable` is substitutable for any other. The unlock algorithm doesn't care whether it's unlocking a chest or a door. Later, if you add a locked journal, a portal, or a gate — all the unlock logic works for free, because they all implement the same interface.

> **Try this:** look at `GameEngine.TryUnlockChest` and `GameEngine.TryUnlockDoor`. They're nearly identical copy-paste twins. Your stretch goal could be to merge them into a single `TryUnlockTarget(ILockable)` helper.

---

## Project Structure

```
W14-assignment-template.sln
│
├── ConsoleRpg/                           # UI & Game Logic
│   ├── Program.cs
│   ├── Startup.cs
│   ├── appsettings.json
│   ├── Services/
│   │   └── GameEngine.cs                 # REWRITTEN: Look, Move, PickUp, Drop
│   └── Helpers/
│       ├── MenuManager.cs
│       └── OutputManager.cs
│
└── ConsoleRpgEntities/                   # Data & Models
    ├── Data/
    │   ├── GameContext.cs                # Extended: Room discriminator, Door DbSet
    │   └── GameContextFactory.cs
    ├── Models/
    │   ├── Characters/
    │   │   ├── Player.cs                 # NEW: CurrentRoomId, PassThroughDoor, InspectForSecretDoors
    │   │   └── Monsters/
    │   │       ├── Monster.cs            # NEW: CurrentRoomId
    │   │       └── Goblin.cs
    │   ├── Containers/
    │   │   ├── IItemContainer.cs
    │   │   ├── ILockable.cs              # From W13 - now implemented by TWO entities
    │   │   ├── Container.cs
    │   │   ├── Inventory.cs
    │   │   ├── Equipment.cs
    │   │   ├── Chest.cs                  # From W13
    │   │   ├── MonsterLoot.cs            # From W13
    │   │   ├── Room.cs                   # NEW: Container subclass for rooms
    │   │   ├── Item.cs
    │   │   ├── Weapon.cs
    │   │   ├── Armor.cs
    │   │   ├── Consumable.cs
    │   │   └── KeyItem.cs
    │   ├── World/
    │   │   └── Door.cs                   # NEW: ILockable entity between rooms
    │   └── Abilities/
    ├── Helpers/
    │   ├── ConfigurationHelper.cs
    │   └── MigrationHelper.cs
    └── Migrations/
        ├── BaseMigration.cs
        ├── 20260410182937_InitialCreate.cs       # W12
        ├── 20260410183100_SeedInitialData.cs     # W12
        ├── 20260410192228_AddChestsAndMonsterLoot.cs  # W13
        ├── 20260410192408_SeedWorldContent.cs    # W13
        ├── 20260410201130_AddRoomsAndDoors.cs    # NEW: schema for W14
        ├── 20260410201312_SeedDungeon.cs         # NEW: dungeon layout
        └── Scripts/
            ├── SeedInitialData.sql         # W12
            ├── SeedWorldContent.sql        # W13
            └── SeedDungeon.sql             # NEW (rooms + doors + floor items)
```

---

## The Seeded Dungeon

```
                       ┌──────────────┐
                       │ Hidden Shrine│   (north, secret door)
                       │    (Id 12)   │
                       └──────┬───────┘
                              │
                      [HIDDEN PANEL - secret]
                              │
  ┌──────────────┐     ┌──────┴───────┐     ┌──────────────┐
  │Locked Cellar │─────┤Entrance Hall ├─────┤North Chamber │
  │   (Id 11)    │     │    (Id 8)    │     │   (Id 9)     │
  └──────────────┘     └──────┬───────┘     └──────────────┘
       (west)                 │                 (east)
  [HEAVY IRON DOOR]    [RUNE-ETCHED DOOR]    [STONE ARCHWAY]
     requires              trapped                open
    "cellar-key"       (12 damage once)
                              │
                       ┌──────┴───────┐
                       │ Trapped Vault│
                       │   (Id 10)    │
                       │  + Grubnak   │
                       └──────────────┘
                            (south)
```

**Starting point:** Entrance Hall (Room 8).

**Your progression path:**
1. **East** to the North Chamber — find the **Cellar Key** and a healing potion on the floor
2. **South** through the trapped door — take 12 damage once, reach the Trapped Vault where **Grubnak the Goblin** guards the Golden Scimitar
3. Fight Grubnak (he still drops the Dungeon Key from W13 — useful for W13's chests if any)
4. Return **west** from the Entrance Hall — use the Cellar Key to unlock the Heavy Iron Door and grab the Cloak of Midnight
5. **???** — there's a Hidden Shrine somewhere but you'll need to search for it. See the stretch goal below.

There's also a **Slim Lockpick** lying on the floor of the Entrance Hall — it works on pickable locks that don't require a specific key.

---

## Assignment Tasks

### Task 1: Run the Migrations

```bash
dotnet ef database update --project ConsoleRpgEntities --startup-project ConsoleRpg
```

Two new migrations apply:
1. **`AddRoomsAndDoors`** — adds Room columns to Containers (Name, Room_Description, X/Y, North/South/East/WestRoomId), adds CurrentRoomId to Players/Monsters, creates the Doors table
2. **`SeedDungeon`** — runs `Migrations/Scripts/SeedDungeon.sql` to create the 5 rooms, 4 doors, floor items, and place Elara and Grubnak

> **Tip:** open SQL Server Object Explorer and `SELECT ContainerType, COUNT(*) FROM Containers GROUP BY ContainerType`. You should see FIVE container types sharing one table now: Inventory, Equipment, Chest, MonsterLoot, and Room. That's TPH at its peak.

### Task 2: Walk Around

Run the game. You start in the Entrance Hall. Try each menu option:
- **Look around** — see the exits, items, and visible doors
- **Move** — walk through passages (notice how trapped doors only hurt once!)
- **Pick up item from floor** — grab the lockpick
- **Drop item** — watch it appear on the floor (you'll see it next time you Look)

**Discussion prompt:** when you dropped an item, the database changed. Open SQL Server Object Explorer and query `SELECT * FROM Items WHERE Name = 'YourDroppedItem'`. What does the `ContainerId` point to now?

### Task 3: Read `Player.TryUnlock`

Open `Models/Characters/Player.cs` and compare the Week 14 `TryUnlock` to your memory of the W13 version:

```csharp
// Week 13:
public bool TryUnlock(Chest chest, KeyItem key) { ... }

// Week 14:
public bool TryUnlock(ILockable target, KeyItem key) { ... }
```

Only the parameter type changed. Inside the method body, every `chest.IsLocked` became `target.IsLocked` — otherwise the algorithm is identical. This is the **Liskov Substitution Principle**: the method doesn't care whether it's unlocking a chest or a door, because both implement the same interface.

### Task 4: Your Assignment — "Where Am I?" LINQ Query

Write a method in `GameEngine.cs` called `ShowAllRooms()` that:

1. Uses LINQ to list every room in the world, sorted by Name
2. For each room, shows:
   - The room name
   - Whether the player is currently there (mark with an arrow)
   - The number of items on the floor (`room.Items.Count`)
   - The number of visible exits (use `CollectVisibleExits(room).Count`)
3. Wire it into the main menu as a new option

Example output:
```
Dungeon map:
  Entrance Hall   (items: 1, exits: 3)  <-- YOU ARE HERE
  Hidden Shrine   (items: 1, exits: 0)  [UNDISCOVERED]
  Locked Cellar   (items: 1, exits: 1)
  North Chamber   (items: 2, exits: 1)
  Trapped Vault   (items: 1, exits: 1)
```

LINQ you'll use:
- `_rooms.OrderBy(r => r.Name)` — sort
- `_rooms.Where(r => ...)` — filter
- `.Count()` — counting
- Conditional projection for the "current room" marker

### Task 5: Your Assignment — "Find the Key I Need" LINQ Query

When a door refuses you with `[LOCKED]` and `RequiredKeyId = "cellar-key"`, it would be great to know where that key is.

Write a method `FindKeyLocation(string requiredKeyId)` that:

1. Queries `_context.Items.OfType<KeyItem>()` for any KeyItem where `KeyId == requiredKeyId`
2. For each match, uses `.Include(i => i.Container)` so you can see where it lives
3. Prints: "The [KeyName] is in [ContainerType] [Container.Name or Description]"
4. Wire it into the Move menu as a prompt when the player encounters a locked door they can't open

This teaches EF Core eager-loading AND gives the game a nice "hint" feature.

---

## Stretch Goal (+10%)

**Secret Door Discovery**

There's a Hidden Shrine north of the Entrance Hall, but the door to it is secret. Implement the inspection system so players can find it:

1. The method `Player.InspectForSecretDoors(IEnumerable<Door>)` is already built — it takes a list of doors, finds any secret ones connected to the current room, marks them `IsDiscovered = true`, and returns the list.
2. Your job is to **wire it into the GameEngine**:
   - Add a new main menu option: **"Inspect the room"**
   - When chosen, call `_player.InspectForSecretDoors(_doors)` and save changes
   - After a successful discovery, the door becomes visible in subsequent `LookAround` calls
3. Bonus: add a chance-based version where `Inspect` only finds secret doors some of the time (e.g., 50% per inspection). Use `Random.Shared.NextDouble()`.

**Challenge bonus (+5%):**
Merge `GameEngine.TryUnlockChest` and `GameEngine.TryUnlockDoor` into a single helper:

```csharp
private void TryUnlockTarget(ILockable target)
{
    // one implementation that works for both chests and doors
}
```

You should be able to delete `TryUnlockChest` and `TryUnlockDoor` entirely and replace both callers with the new helper. If you can do that and the game still works, you've truly absorbed the Liskov lesson.

---

## Grading Rubric

| Criteria | Points | Description |
|----------|--------|-------------|
| Migrations Run Cleanly | 15 | Both new migrations apply with no errors |
| Walkthrough | 10 | Can navigate the dungeon, pick up, drop, and fight Grubnak |
| Understands the `Room : Container` Idea | 15 | Can explain why dropping an item is now a single FK change |
| Understands the `ILockable` Reuse | 15 | Can explain why `TryUnlock` works for both chests and doors |
| Task 4: `ShowAllRooms` LINQ | 20 | Sorted query with item/exit counts and "you are here" marker |
| Task 5: `FindKeyLocation` LINQ | 15 | Uses `Include` to eager-load the key's container and prints its location |
| Code Quality | 10 | Clean, readable, follows existing patterns |
| **Total** | **100** | |
| **Stretch: Secret Door Inspection** | **+10** | Wires `InspectForSecretDoors` into the game menu |
| **Challenge: Merged Unlock Helper** | **+5** | Single `TryUnlockTarget(ILockable)` replaces both chest and door versions |

---

## How This Connects to the Final Project

W14 is your **last content week**. Everything you've built over the semester is now in place:

- **W12** — Items in containers
- **W13** — More containers, lockable state, monster loot
- **W14** — Items live in a navigable world with doors

The `w15-final` template takes this foundation and adds:
- A **Spectre.Console ASCII minimap** using the Room `X` and `Y` coordinates you already have in the seed data
- **Multiple monsters** roaming between rooms
- **An expanded combat system** with ability usage and status effects
- **A richer seed world** with more rooms, more doors, and more items

Take a look at `w15-final/ConsoleRpg/Helpers/MapManager.cs` to see how a full-featured visual map can be layered on top of the data model you already have. Nothing about the model changes — it's just a different way of presenting the same rooms and doors.

---

## Tips

- **Room is a Container.** When you want items on the floor, query `currentRoom.Items`. When you want to drop an item, `item.ContainerId = currentRoom.Id`.
- **Door is NOT a Container.** Doors don't hold items. They're a separate table with two FKs to Containers (RoomA and RoomB).
- **One door, two rooms.** When navigating, use `_doors.FirstOrDefault(d => (RoomA=from,RoomB=to) || (RoomA=to,RoomB=from))` to match either approach direction.
- **`Room_Description` quirk:** Chest already has a `Description` column on the Containers table. EF Core disambiguates Room's Description by renaming it to `Room_Description` in SQL. If you write raw SQL, use that name; if you use LINQ/EF, use `room.Description` as normal.
- **Secret doors are not missing doors.** An `IsSecret` door is a real row in the Doors table with both `RoomAId` and `RoomBId` set. The `IsVisible` helper hides it until discovered. That way data integrity is guaranteed (every door connects two real rooms).
- **Use `Include(p => p.CurrentRoom!).ThenInclude(r => r.Items)`** when loading the player, so `player.CurrentRoom.Items` works without additional round-trips.

---

## Submission

1. Commit your changes with a meaningful message
2. Push to your GitHub Classroom repository
3. Submit the repository URL in Canvas

---

## Resources

- [EF Core Self-Referencing Relationships](https://learn.microsoft.com/en-us/ef/core/modeling/relationships/self-referencing)
- [Liskov Substitution Principle (Wikipedia)](https://en.wikipedia.org/wiki/Liskov_substitution_principle)
- [EF Core Eager Loading with Include](https://learn.microsoft.com/en-us/ef/core/querying/related-data/eager)
- [Enumerable.OfType<TResult>](https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.oftype)

---

## Need Help?

- Post questions in the Canvas discussion board
- Attend office hours
- Review the in-class repository for additional examples