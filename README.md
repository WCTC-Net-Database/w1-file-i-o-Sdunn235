# Week 13: Chests & Monster Loot — Implementation

> **Student:** Shawn Dunn
> **Submitted:** 2026-04-28
> **Database:** `w9_efcore_SDunn` on `bitsql.wctc.edu`
> **Migrations:** `W13_AddChestsAndMonsterLoot` (schema) + `W13_SeedWorldContent` (data)
> **Solution:** `w13-chest-loot.sln` (continues the W12 codebase)

---

## Overview

W13 demonstrates the **Open/Closed Principle** by extending the W12 `Container`
TPH with two new subclasses — `Chest` and `MonsterLoot` — without modifying any
W11 or W12 code. It also introduces **`ILockable`** as a separate interface so
chests advertise *two* capabilities (holds items, can be locked) without
collapsing them into one bloated contract. W14 will reuse `ILockable` on `Door`.

The graded LINQ tasks (Richest Locked Chest, DisarmTrap) are wired into a new
**Chest Interaction** submenu (option 15), gated on the active character being
a `Player`.

A small **LucentForge integration** is layered onto `TryUnlock`: lockpicking
becomes a real skill check using `Reflexes` + `CharacterSkill.Proficiency` for
the seeded `Lockpicking` skill row, rolled against `Chest.UnlockDC`. Specific
keys (e.g., `Dungeon Key` with `KeyId = "dungeon-main"`) bypass the roll
entirely. This is the *only* LF integration scoped to W13 — magic-warded
chests and combat are deferred to W14/W15.

---

## Learning Objectives — Status

- [x] Extend a TPH hierarchy with new subclasses without changing existing rows
- [x] Apply the Open/Closed Principle in a real EF Core model change
- [x] Separate two concerns with two interfaces (`IItemContainer` + `ILockable`)
- [x] Implement state-based logic (locked → unlocked, trapped → disarmed)
- [x] Work with an `enum` return type to model multiple outcomes (`OpenResult`)
- [x] Use LINQ `OfType<T>()` to query a TPH hierarchy by concrete type
- [x] Write and run two migrations — one for schema, one for seed data
- [x] **Graded Task A:** Richest Locked Chest (25 pts)
- [x] **Graded Task B:** `Player.DisarmTrap` (25 pts)

---

## Design Deviations (Justified)

| Template Approach | My Implementation | Reason |
|-------------------|-------------------|--------|
| `KeyItem` as its own `Item` TPH subclass | `Item.IsKeyItem` bool + `Item.KeyId` nullable string | W12 committed `IsKeyItem` as a bool. Adding a TPH subclass would force re-keying the Items table. The same lockpick-vs-specific-key distinction is captured by `KeyId == null` (lockpick) vs `KeyId != null` (specific key). |
| `Equipment.CanEquip` checks `Items.Any(...)` | `Equipment.CanEquip` checks `Slots.Any(...)` | W12 shipped slot-based equipping (`EquipmentSlot` rows owned by `Equipment`). The slot collection lets the invariant check the *specific* slot is empty, not just "no item with that EligibleSlot exists in the bag". More rigorous; preserves W11/W12 work. |
| `Monster.LootId` (the template assumes a Monster class) | `Npc.LootId` + `MonsterLoot.IsLooted` | W11 already shipped Goblin as a `Race` rather than a Character TPH subclass. There is no Monster class — Grubnak is an `Npc` with `Race.Name = "Goblin"`. `IsLooted` lives on the loot container itself, not the NPC, keeping the NPC table clean and the loot state co-located with the loot. |
| Lockpicking is a flat boolean (`IsPickable`) | Lockpicking is a Skill check (`Reflexes` + `CharacterSkill.Proficiency` vs `Chest.UnlockDC`) | LucentForge integration. The W11 `Skills` and `CharacterSkills` tables already exist; this gives them their first real consumer. Specific keys still bypass the roll, so the rubric path through W13 is preserved. |

---

## What's New in W13

### New files

| Path | Purpose |
|------|---------|
| `ConsoleRpgEntities/Models/Containers/ILockable.cs` | Interface — `IsLocked`, `IsTrapped`, `IsPickable`, `RequiredKeyId`, `TrapDamage`, `TrapDisarmed`, `UnlockDC` |
| `ConsoleRpgEntities/Models/Containers/Chest.cs` | `: Container, ILockable` — adds `Description`, `RoomId`, `Room` nav |
| `ConsoleRpgEntities/Models/Containers/MonsterLoot.cs` | `: Container` — adds `IsLooted` |
| `ConsoleRpgEntities/Models/Containers/OpenResult.cs` | Enum — `Opened`, `Locked`, `Trapped`, `AlreadyOpen` |
| `ConsoleRpgEntities/Migrations/Scripts/W13_SeedWorldContent.sql` | Idempotent seed: rooms, chests, Grubnak + loot, Lockpicking skill + Elara's proficiency, lockpicks |
| `ConsoleRpgEntities/Migrations/Scripts/W13_SeedWorldContent.rollback.sql` | Tear-down used by `Down()` |

### Modified files

| Path | Change |
|------|--------|
| `ConsoleRpgEntities/Models/Items/Item.cs` | + `KeyId` nullable string; + `EligibleSlot` virtual `[NotMapped]` |
| `ConsoleRpgEntities/Models/Items/Weapon.cs` | + `EligibleSlot` override → `MainHand` |
| `ConsoleRpgEntities/Models/Items/Armor.cs` | + `EligibleSlot` override → maps `BodySlot` to `SlotType` |
| `ConsoleRpgEntities/Models/Containers/Equipment.cs` | + `CanEquip(item)` — slot-based invariant |
| `ConsoleRpgEntities/Models/Npc.cs` | + `LootId` nullable int + `Loot` (MonsterLoot) nav |
| `ConsoleRpgEntities/Models/Player.cs` | + `OpenChest`, `TryUnlock`, `DisarmTrap`, `LootChest`, `LootMonster` |
| `ConsoleRpgEntities/Data/GameContext.cs` | + Chest/MonsterLoot discriminators, Chest→Room FK, Npc→MonsterLoot 1:1 |
| `ConsoleRpg/Services/GameEngine.cs` | + `ChestMenu` + 7 sub-options including the two graded LINQ tasks |
| `ConsoleRpg/UI/ConsoleGameUi.cs` | Welcome banner → W13; main menu adds option 15 |
| `ConsoleRpg/Program.cs` | Wires option 15 → `ChestMenu` |
| `w12-efcore-adv.sln` → `w13-chest-loot.sln` | Solution file renamed |
| `ConsoleRpg/Properties/launchSettings.json` | Profile `w6-solid-dip` → `w13-chest-loot` |

---

## Relationships Configured

| Principal | Dependent | Cardinality | FK | On Delete |
|-----------|-----------|-------------|----|-----------|
| `Container` | (Chest discriminator) | TPH | `ContainerType = 'Chest'` | n/a |
| `Container` | (MonsterLoot discriminator) | TPH | `ContainerType = 'MonsterLoot'` | n/a |
| `Room` | `Chest` | 1 : many | `Chest.RoomId` (nullable) | `SetNull` |
| `Npc` | `MonsterLoot` | 1 : 0..1 | `Npc.LootId` (nullable) | `SetNull` |

The `ContainerType` discriminator now has four values (Inventory, Equipment,
Chest, MonsterLoot). Adding the two W13 values is a five-line change to
`OnModelCreating` — that's the OCP payoff.

---

## Graded LINQ Tasks

### Task A — Richest Locked Chest

```csharp
var richest = _dbContext.Containers
    .OfType<Chest>()
    .Where(c => c.IsLocked)
    .OrderByDescending(c => c.ItemsCollection.Sum(i => i.Value))
    .FirstOrDefault();
```

With seeded chests, the Iron-Banded Chest (Silvered Shortsword 65g + Leather
Bracers 25g = **90g**) wins until you find the Dungeon Key on Grubnak — at
which point unlocking the Ornate Rune-Engraved Chest (~770g) takes it out of
the locked set.

### Task B — `Player.DisarmTrap(chest, lockpick)`

```csharp
public bool DisarmTrap(Chest chest, Item lockpick)
{
    if (!lockpick.IsKeyItem || lockpick.KeyId is not null) return false;
    if (!chest.IsTrapped || chest.TrapDisarmed) return false;

    chest.TrapDisarmed = true;
    Inventory?.RemoveItem(lockpick);
    return true;
}
```

Lockpick-only (`IsKeyItem && KeyId == null`), trapped-chests-only,
`TrapDisarmed = true` on success, lockpick consumed. Wired into the Chest
Interaction submenu so the player can defuse the Dusty Humming Chest before
opening it (otherwise `OpenChest` fires the trap for 8 damage).

---

## LucentForge Integration (scoped)

The single integration this week: **lockpicking is a real skill check.**

- Seed adds a `Lockpicking` skill row (`PrimaryAttribute = Reflexes`) and
  Elara's `CharacterSkill` row with `Proficiency = 3`.
- `Player.TryUnlock` lockpick branch rolls `1d20 + Reflexes + Proficiency`
  vs. `Chest.UnlockDC`. Lockpick is consumed regardless. Specific keys
  (`KeyId != null`) bypass the roll entirely.
- Chest 2 (`UnlockDC = 12`) is reachable for Elara (Reflexes 7 + Proficiency 3 = 10
  before the d20 — needs ≥2 on the die). Chest 3 (`UnlockDC = 99`) is
  effectively unpickable; only the Dungeon Key opens it.

Magic-warded chests, combat resolution before `LootMonster`, and race-driven
modifiers are *not* part of W13 — those land in a separate W14/W15 plan
(see the Caelum framework's `projects/` directory).

---

## Carry-forward polish (from the W11 → W12 follow-up list)

- **Item 4 (active-character forcing):** Chest interaction is `Player`-only and
  routes through `ResolveActivePlayer()`, which prompts for selection if no
  active character is set.
- **Item 7 (M:M join doc for grader):** documented below in the *M:M Join
  Tables* appendix.

(Item 6 — Consumable Effect/Potency help text — was not addressed this module.)

---

## Migrations

### `W13_AddChestsAndMonsterLoot` (schema)

Pure additive — the `Containers` table gains 9 new columns
(`Description`, `IsLocked`, `IsTrapped`, `IsPickable`, `RequiredKeyId`,
`TrapDamage`, `TrapDisarmed`, `UnlockDC`, `RoomId`, `IsLooted`), all nullable
because they only apply to specific TPH subclasses. `Items` gains `KeyId`
(nullable). `Characters` gains `LootId` (nullable, unique-filtered FK to
Containers). No existing column types or rows are touched.

### `W13_SeedWorldContent` (data)

Runs `Migrations/Scripts/W13_SeedWorldContent.sql` via the W12 `BaseMigration`
+ `MigrationHelper.RunSqlScript` plumbing. Seeds:

- **Rooms:** Antechamber, Vault. Elara is moved to the Antechamber if she has no room.
- **Skill:** `Lockpicking` (PrimaryAttribute = Reflexes) + Elara's `CharacterSkill` proficiency.
- **Chests (4):**
  - *Weathered Wooden Chest* — open, contains `Lesser Healing Draught`, `Rusty Dagger`
  - *Iron-Banded Chest* — locked (`UnlockDC = 12`, pickable), `Silvered Shortsword`, `Leather Bracers`
  - *Ornate Rune-Engraved Chest* — locked, NOT pickable, `RequiredKeyId = "dungeon-main"`, `Ember Wand`, `Mithril Chainmail`, `Elixir of the Wakeful`
  - *Dusty Humming Chest* — trapped (`TrapDamage = 8`), `Trapmaker's Dagger`, `Antidote`
- **Grubnak the Goblin (Npc):** placed in the Vault, with a `MonsterLoot`
  container holding `Goblin Cleaver`, `Dungeon Key` (KeyId = `dungeon-main`),
  `Gobbo's Stew`. Treated as already-defeated per the W13 cosmetic-loot
  decision — combat lands in a later module.
- **Lockpicks:** two `Iron Lockpick` consumables seeded into Elara's inventory.

Every insert is guarded with `NOT EXISTS` keyed on stable Names — safe to
re-run against the shared DB.

---

## Running the Game

```
15. Chest Interaction          ← new W13 submenu
  1. List chests in current room
  2. Open chest
  3. Try unlock chest (key or lockpick)
  4. Disarm trap (lockpick) — graded
  5. Loot chest
  6. Loot defeated monster
  7. Richest locked chest (graded)
  0. Back to main menu
```

Suggested walkthrough:

1. **Select Character** → `Elara`. (Active header shows `[Active: Elara the Bold (Player)]`.)
2. **Chest Interaction → 1.** Lists the 2 chests in the Antechamber.
3. **Chest Interaction → 2** on the Wooden Chest → opens trivially.
4. **Chest Interaction → 3** on the Iron-Banded Chest with a lockpick → skill check.
   On success, **5.** loots the silvered sword + bracers.
5. **Move Player** → North/whatever exit leads to the Vault. (Or seed-add a door later.)
6. **Chest Interaction → 4** on the Dusty Humming Chest with a lockpick → trap disarmed.
   Then **2** to open, then **5** to loot.
7. **Chest Interaction → 6** on Grubnak → looted; gain the Dungeon Key.
8. **Chest Interaction → 3** on the Ornate Chest using the Dungeon Key → unlocks.
   **5** to loot the Ember Wand + Mithril Chainmail + Elixir.
9. **Chest Interaction → 7** at any point shows the richest locked chest still standing.

---

## Rubric Self-Assessment

| Criterion | Weight | Status | Notes |
|-----------|--------|--------|-------|
| Migrations Run Cleanly | 15 | ✅ | Both migrations applied to `w9_efcore_SDunn` with no errors |
| Understands the OCP Pattern | 15 | ✅ | New discriminators added; W11/W12 code untouched. See *Design Deviations* |
| Understands `ILockable` Separation | 10 | ✅ | `MonsterLoot` deliberately omits `ILockable`. Chest implements both. ISP discussed in *Overview* |
| Task A: Richest Chest LINQ | 25 | ✅ | `OfType<Chest>().Where(...).OrderByDescending(c => c.Items.Sum(...)).FirstOrDefault()` |
| Task B: `DisarmTrap` Method | 25 | ✅ | Lockpick-only, trapped-only, consumes lockpick, integrates with chest flow |
| Code Quality | 10 | ✅ | Matches W11/W12 patterns; lazy-loading proxies, virtual nav properties, no `Include()` calls |
| **Target** |  | **100** |  |
| Stretch: Drop Tables | +10 | ⚪ | Deferred — Grubnak's loot is hardcoded for W13. Drop tables are a natural W14/W15 fit once combat lands |

---

## M:M Join Tables (grader appendix)

The `Character` ↔ `Ability` and `Character` ↔ `Magic` relationships use the
EF Core "skip navigation" pattern. The shadow join tables are configured in
`GameContext.OnModelCreating`:

```csharp
modelBuilder.Entity<Character>()
    .HasMany(c => c.Abilities)
    .WithMany(a => a.Characters)
    .UsingEntity(j => j.ToTable("CharacterAbilities"));

modelBuilder.Entity<Character>()
    .HasMany(c => c.Magics)
    .WithMany(m => m.Characters)
    .UsingEntity(j => j.ToTable("CharacterMagic"));
```

The `Character` ↔ `Skill` relationship is an *explicit* join entity
(`CharacterSkill`) because it carries a payload (`Proficiency`). That's the
table the W13 LucentForge integration writes to when seeding Elara's
Lockpicking skill row.

---

## How This Connects to W14+

| Week | What gets added | What carries over from W13 |
|------|-----------------|----------------------------|
| **W14** | `Room` may grow into another Container subclass; `Door` implements `ILockable` | `Player.TryUnlock` already handles ILockable — W14 reuses it on doors. The lockpick skill check ports directly. |
| **W15+** | Combat resolution; magic-warded chests via spell cost; race-flavored loot rules | The `OpenResult` enum will gain `Warded`/`Resisted`. `MonsterLoot.IsLooted` will be set by the combat system instead of a menu choice. |

---

## Verification Steps

1. `dotnet build w13-chest-loot.sln --configuration Release` — 0 warnings, 0 errors
2. `dotnet ef database update --project ConsoleRpgEntities --startup-project ConsoleRpg` — both migrations apply cleanly
3. SSMS:
   - `SELECT ContainerType, COUNT(*) FROM Containers GROUP BY ContainerType` → Inventory, Equipment, Chest, MonsterLoot rows
   - `SELECT Name, KeyId FROM Items WHERE IsKeyItem = 1` → Old Brass Key (NULL), Dungeon Key (`dungeon-main`), Iron Lockpick #1/#2 (NULL)
   - `SELECT * FROM CharacterSkills cs JOIN Skills s ON cs.SkillId = s.Id WHERE s.Name = 'Lockpicking'` → Elara, Proficiency 3
4. App walkthrough: follow the *Running the Game* sequence above; observe richest-chest output before and after looting the Ornate Chest.
