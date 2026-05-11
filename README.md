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

#### Polish — Loot output detail + interactive picker

The W13 loot path summarized everything into "Looted N items from
{source}" — a count without item names. Looting Grubnak told the player
they got *something* but never *what*. Replaced the count-only output
with a real RPG-style picker.

**Architecture (SOLID + GRASP):**
- New `Character.TakeItemFrom(Container source, Item item) → bool`.
  Single-Responsibility primitive: move one item from any container
  into this character's inventory, refuse on weight overflow, refuse
  if the source doesn't contain it. The verb is "take," not "loot" —
  so the same method serves chest looting, body looting, picking up
  items off a room's floor (W14), and any future thing-that-holds-items.
  Open/Closed: a new Container subclass needs zero changes here.
- New `GameEngine.LootInteractive(looter, source, sourceLabel)`.
  Numbered list of source items with weight + value tags; player picks
  by number, types `all` to grab everything that fits, or `0` to leave.
  Re-displays after each take. Items the looter can't fit show a
  `[too heavy]` flag and are skipped on `all`. This is the *UX* layer
  on top of `TakeItemFrom`.
- Removed `Character.LootChest` and `Character.LootMonster`. Their only
  callers (`ChestLoot`, `ChestLootMonster`) are rewritten to use
  `LootInteractive`. No back-compat shim — single-source rule.

**Behavior change:**
- "Looted N items from Grubnak" → numbered picker showing every item
  with name, type, weight, value, and a `[too heavy]` tag where it
  applies. Each pick is named in the confirmation: "Took Iron Lockpick."
- `MonsterLoot.IsLooted` is set when the player exits the picker
  (chose `0` or the source was emptied), not when the count happens
  to be > 0. "The body has been searched" semantics.

#### Polish — Item listing shows Owner name, not raw container ID

`ListItems` (Menu 2 → 1) used to print `container #5` next to every
item — surfacing the FK directly to the user, which is unreadable and
leaks schema. Now reads the container's polymorphic `.Name` and labels
it `Owner`:

```
[3] Healing Potion — Consumable, 1 lbs, 25g — Owner: Elara's Pack
[12] Iron Lockpick #1 — Consumable, 1 lbs, 5g — Owner: Elara's Pack
[18] Dungeon Key — Consumable, 1 lbs, 0g — Owner: Grubnak's Pouch
```

The `Owner` label deliberately works across every Container subtype:
characters' inventory bags, chests, monster loot, room floors, and any
future thing-that-holds-items (a desk, a sword in the stone) — they
all expose `Container.Name`, so the display is one-line polymorphic.
The TPH that started in W12 is now paying for itself in UX.

Lazy-loading proxies materialize the Container on access, so this
costs N small queries on a list call — fine for the dev/editor menu;
not worth pre-fetching with `Include` (per the project's standing rule
against eager-loading).

#### Pre-W14-grading polish — committed default points at LocalDB

`appsettings.json` (the committed, public default) used to point at
`bitsql.wctc.edu` with `User Id=PLACEHOLDER`/`Password=PLACEHOLDER`,
which meant a fresh clone couldn't run anything until the cloner
manually created a `appsettings.Development.json` with credentials.
Switched to:

```json
"GameDb": "Server=(localdb)\\MSSQLLocalDB;Database=w9_efcore_SDunn;Trusted_Connection=True;TrustServerCertificate=True;"
```

A grader who clones the repo on a Windows machine with Visual Studio
(LocalDB ships with VS) can now run `dotnet ef database update` and
`dotnet run` immediately — no credential setup, no school VPN.
Real credentials (or alternate DB targets) still live in the
gitignored `appsettings.Development.json` for whoever needs them.
Triggered by a school-side server outage that made
`bitsql.wctc.edu` unreachable; landed as a separate single-file commit
to keep history clean.

#### Phase C.2 — Door bidirectional + ILockable

After Phase C.1 made Room a Container subclass, the Door entity still
modeled passages as a *directional pair*: `SourceRoomId` + `DestinationRoomId`
+ a `Direction` enum, with two rows per bidirectional passage (one
"north from A to B," another "south from B to A"). That model leaks
the schema's bookkeeping into player UX ("go north" must consult the
right row depending on which side you're standing on) and makes lock
state a sync problem (lock both rows or risk one-way locked doors).

| Old approach (W11/W13) | New approach (Phase C.2) | Reason |
|---|---|---|
| Door has `SourceRoomId`/`DestinationRoomId`/`Direction`; two rows per passage. | Door has `RoomAId`/`RoomBId`; one row per passage. `MovePlayer` shows the player a list of doors in the current room and lets them pick. | Single source of truth for lock/trap state per passage. UX freed from the schema's old shape — "the player sees doors, not directions." Stronger LSP narrative for Phase C.4: doors and chests both have ONE state object, not a pair. |
| Direction enum lived on the Door entity. | Direction column dropped. The door doesn't *have* a direction; navigation is relative to the character's current room. | Every door in a bidirectional model has two valid "directions" depending on which side you're on. Encoding only one is leaky; encoding both is redundant. The simpler answer is to drop the concept entirely. |
| `MovePlayer` matched `door.Direction == requested` to find the door for "go north." Required Direction enum to remain meaningful. | `Door.GetOtherRoom(currentRoom)` returns the room on the opposite side from where the character is now. `MovePlayer` asks the door directly. | **GRASP information-expert pattern** (Shawn's design call): the Door owns its endpoints, so it answers "which side is opposite mine." `MovePlayer` doesn't reason about A vs B — it hands the door the current room and uses the result. The class with the data does the work. |
| EF scaffolded the migration as `RenameColumn(SourceRoomId → UnlockDC)` because both columns are int. | Hand-edited migration: `DropColumn(Direction)` first, then `RenameColumn(SourceRoomId → RoomAId)` and `RenameColumn(DestinationRoomId → RoomBId)`, then add the eight new ILockable + secret-door fields with sensible defaults. | EF scaffolds by *type compatibility*, not semantic intent. `SourceRoomId` (FK int) and `UnlockDC` (lock difficulty int) happen to share a type but mean wildly different things; the scaffolded version would have moved Room IDs into a difficulty-class field. Reviewing every scaffolded migration is non-negotiable. |
| Door had only `IsLocked`. | Door implements `ILockable`: `IsLocked`, `IsTrapped`, `IsPickable`, `RequiredKeyId`, `TrapDamage`, `TrapDisarmed`, `UnlockDC`, plus stretch fields `IsSecret` / `IsDiscovered` (with computed `IsVisible`). | Door is now the structural cousin of Chest. Phase C.4's `TryUnlock(ILockable, ...)` LSP refactor will exercise this — the same unlock algorithm will work for chests and doors with zero duplication. The W14 README's "single tiny change" payoff lands here. |

**Verification (against fresh LocalDB):** all 12 migrations applied
clean. Antechamber + Vault present after C.1's MERGE-based seed
preservation. Adding a test door between them via menu 3 → 5 → 2
prompts for Room A / Room B / Name / Description / Locked? / Secret?
(no Direction). Moving an active character via menu 3 → 7 lists doors
by name with destination, no direction prompt. Grubnak's looted
inventory still surfaces correctly through the Phase 1.5 → B → B.1 →
C.1 chain.

**Composability tee-up for Phase C.3:** Door has `RequiredKeyId` (string),
matching Chest's. Phase C.3 extracts `KeyItem : Item` so this column
joins to a typed entity instead of a free-text identifier — the last
"keys are stringly typed" smell goes away in the same commit that
finally drops `Item.IsKeyItem`.

#### Phase C.1 — Room as 5th Container TPH subclass

| Template / default approach | My implementation | Reason |
|---|---|---|
| EF scaffolded `DropTable("Rooms")` outright. Would have lost all room data and orphaned `Character.RoomId`, `Door.SourceRoomId`/`DestinationRoomId`, and `Chest.RoomId`. | Hand-edited migration uses `MERGE...OUTPUT INTO #RoomMap` to atomically insert each Room into Containers as a `ContainerType='Room'` discriminator while capturing the old→new id mapping. Then UPDATE every FK column via the mapping before dropping the old Rooms table. | Preserves Antechamber + Vault (and any future authored rooms) through the schema change. The mapping pattern is reusable for future TPH promotions. |
| Both `Character.RoomId` and `Chest.RoomId` keep `OnDelete(SetNull)` after pointing at the new Containers table. | Changed both to `NoAction`. | Combining two `SetNull` cascades on a self-referential `Containers` table triggers SQL Server's "multiple cascade paths" guard (error 1785). `RemoveRoom` in `GameEngine` already explicitly nulls Character.RoomId, removes connected Doors, and cleans Chest.RoomId before the actual delete, so DB cascade isn't load-bearing. |
| Room kept in `Models/Room.cs` (its original location, alongside Character/Door/etc.). | Moved Room to `Models/Containers/Room.cs` to live with its TPH siblings (Inventory, Equipment, Chest, MonsterLoot). Door updated with the new `using ConsoleRpgEntities.Models.Containers;`. | Code organization follows the data model: containers live together. One-line consumer change worth the structural clarity. |
| `Description` column collides with `Chest.Description` in TPH. | Let EF auto-shadow as `Room_Description` (the "Room_Description quirk" the W14 README mentions). Documented in the Room.cs XML doc so future readers don't trip on raw SQL. | Default EF behavior; cleaner than custom `[Column]` overrides. Cost is one quirk to remember; benefit is no schema customization. |

**Verification:** App starts clean; integrity sweep runs; Room list (menu 3 →
1) shows Antechamber + Vault preserved; characters and chests still know
their room. Floor-items semantics now available via `room.AddItem(item)` and
`room.ItemsCollection` — same shape as backpacks, chests, equipment slots,
and monster loot. The "Room IS a Container" idea is the literal data model.

**Composability tee-up for Phase C.2:** Door currently has
`SourceRoomId/DestinationRoomId/Direction` (directional pair model — two
rows per bidirectional passage). Phase C.2 will refactor to bidirectional
(`RoomAId/RoomBId`, single row per passage) and implement `ILockable`,
turning Door into the structural cousin of Chest under one shared
lock/trap interface.

#### Phase C.2.5 — `AddItem` placement picker for created items

`GameEngine.AddItem` (Item Management menu → Add Item) used to save every
newly created item with `ContainerId = NULL` — i.e., always "unowned." There
was no in-menu way to place a freshly created item into a specific
container; the only ways to populate a Chest, Monster loot pouch, or Room
floor with a custom item were (a) the seed SQL scripts at migration time
or (b) hand-editing the `Items.ContainerId` column. Test scenarios that
need a specific item somewhere — encumbrance verification (heavy item in a
chest, can the player refuse the loot?), lock/trap testing, future floor-
pickup tests — all forced the table-edit workaround.

| Old approach | New approach (Phase C.2.5) | Reason |
|---|---|---|
| `AddItem` saves with `ContainerId = NULL` unconditionally. Placement requires editing the `Items` table by hand. | After the item is constructed and before `SaveChanges`, a `PromptItemPlacement(item)` helper offers a numbered picker: 0. Unowned (default) / 1. Character inventory / 2. Chest / 3. Monster loot / 4. Room floor. Each sub-choice lists the existing containers of that subtype and sets `item.ContainerId` to the chosen container's PK. | Every Container TPH subtype is reachable from one in-game flow. The W12 promise that "items live in a single Items table with a single ContainerId FK" finally has matching CRUD UX — placing an item is the same op regardless of where it goes. |
| To verify encumbrance refusal (`Character.CanCarry` → `LootInteractive`'s `[too heavy]` tag), Shawn had to seed a heavy item into a chest by SQL before running. | Create heavy item → place in Antechamber's chest in the same flow → walk Elara up → menu Chest → Loot → see the `[too heavy]` refusal. | Self-contained verification path for the C0029 encumbrance fix. Eliminates the SQL-edit step from the test loop. |
| Room floor placement is reachable from the picker, but Phase C.2 left no in-game pickup UI for room floors (only `MovePlayer` and `DisplayCurrentRoom` exist under Room submenu). | The picker still allows Room-floor placement for seeding purposes; the floor-pickup wiring is deferred to Phase D alongside `ShowAllRooms` / `FindKeyLocation`. | The placement primitive is cheap once, useful immediately for the chest/loot flows, and a step closer to the Phase D floor-pickup work. No reason to gate it behind the missing pickup verb. |

**Verification:**
- App starts clean.
- Menu Item Management → 2. Add Item → create a Weapon "Dark Matter" weight 200 → at the placement prompt pick `2. Chest` → pick Antechamber's chest → confirm `created (in container #N)` line and SQL `SELECT ContainerId FROM Items WHERE Name = 'Dark Matter'` returns the chest's PK.
- Walk Elara to that chest → menu Chest → Loot → confirm Dark Matter shows `[too heavy]` in the picker and a numbered selection prints `Couldn't take Dark Matter — too heavy.` This pairs with C0029's encumbrance fix as the verification path.

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