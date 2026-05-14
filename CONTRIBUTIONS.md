# CONTRIBUTIONS

> **Required for ALL tiers.** Replace the bracketed prompts with your own
> answers. Honesty is graded; volume is not. One paragraph per section is
> usually enough — be specific, not impressive.

---

## 1. Starting Point

I started from my own W14 repo (`w1-file-i-o-Sdunn235`, branch `main`,
tip commit C0034) and carried it forward directly into the W15 module
folder. The W15 template README and this `CONTRIBUTIONS.md` template
were added as scaffolding only — none of the template's implementation
files (`MapManager.cs`, `ExplorationUI.cs`, `PlayerService.cs`,
`AdminService.cs`, `ParserDemo.cs`) were used. The template migrations
(`AddMonsterTypes`, `AddChestLocation`, `SeedFinalWorld`) were not
applied; all migrations in this repo are original W12–W15 work.

The starting codebase brought everything built in W12–W14:
- Container TPH (Inventory / Equipment / Chest / Room) — MonsterLoot was present at W14 tip and eliminated in W15 Phase E
- Door bidirectional + ILockable (Phase C.2)
- TryUnlock/DisarmTrap LSP refactor (Phase C.4)
- KeyItem sentinel via `KeyId` column (Phase C.3-lite)
- Graded LINQ — ShowAllRooms + FindKeyLocation (Phase D)
- Spectre.Console Panel + Table intro (Phase D)
- W14_SeedC4Demo world: Antechamber, Vault, Hidden Alcove, 3 doors,
  4 chests, Grubnak the Goblin

---

## 2. What I Added

### W15 Submission (C0035–C0042, graded 115/115)

- **Phase E — MonsterLoot eliminated (C0036):** Removed `MonsterLoot` as a separate Container subclass.
  NPCs now own their loot in their `Inventory`. Hand-edited migration moves items before dropping columns.
- **Gobby the Outcast Goblin:** Renamed Grubnak to Gobby. Narrative thread: exiled from his tribe,
  guarding the vault key alone.
- **LockedJournal — ILockable on an Item (C0037):** `LockedJournal : Item, ILockable`. Proves
  `TryUnlock` works on a third host type with zero changes to the method. The LSP payoff in action.
- **Bookshelf + Tome (C0038):** `Bookshelf : Container` and `Tome : Item` with `LoreText`. Seeded
  in Ancient Library. Books contradict Gobby's journal — two accounts, neither wins.
- **Wolf NPC subtype (C0039):** `Wolf : Npc` with `PackSize` property. Seeded at Forest Edge.
  Proves Phase E generalizes beyond Gobby.
- **LINQ queries submenu (C0040):** InventoryAudit (GroupBy container type), MostDangerousRoom
  (GroupBy room / Sum HP), LockedTreasures (ILockable entities player can't open — spans all 3 types).
- **SelectCharacter + menu sections (C0041):** Numbered character list, Game/Admin menu grouping.

### Post-Grade Polish (C0043–C0055)

These commits extend the project toward a playable dungeon demo and LucentForge foundation.

- **C0043 — Mode split + bug fix:** Play/Admin mode gate on startup. Bug fix: `ChestRichestLocked`
  crash (`.ToList()` before accessing lazy-loaded `ItemsCollection`).
- **C0044 — [Flags] enums:** `TrapType [Flags]` (teacher-suggested: Mechanical/Magical/Poison/Electric).
  `SlotType` redesigned to power-of-2 values with `[Flags]`; `AnyHand = MainHand | OffHand`. Migration
  converts old sequential ints to new power-of-2 values without data loss.
- **C0045 — Player loop + dialogue:** Full room-first `PlayerLoop()`. Spectre Panel room display with
  color HP bar. Numbered exits + lettered room objects with type dispatch. NPC dialogue: Mira (shop +
  cellar quest + reward), Gobby (3 HP-gated branches), Erasmus (boss monologue). Combat expanded with
  BitPool/BytePool split.
- **C0046 — Grid coordinates + mini-map:** `Room.GridX`/`GridY`. ASCII mini-map in Spectre Panel.
  All connections must be orthogonal for connector rendering.
- **C0050 — Combat bug fixes:** Magic filter restricted to attack-kind spells; POW label corrected;
  BitPool/BytePool draw from correct resource pool.
- **C0051 — 7-room level redesign:** Complete world replacement migration. Inn as start, Cellar for
  Mira's quest, Goblin Camp for key acquisition, Chapel Gate locked+trapped, Crypt, Vault boss.
  Fixed Bookshelf FK column (`[Bookshelf_RoomId]`) and Door Description NOT NULL constraint.
- **C0052 — Mira shop markup fix:** `Markup.Escape()` + `[[...]]` for item names/descriptions in
  Spectre SelectionPrompt (bracket characters were parsed as color tags).
- **C0053 — Playtest bug fixes:** Data Mend attacking fixed (attack-kind filter), Gobby HP reset
  via migration, grid coordinates redesigned to orthogonal layout, Chapel Shelves showing all items
  (not just `OfType<Tome>()`), lockpick-on-non-pickable door shows key hint.
- **C0054 — PauseAndClear pattern:** All interaction handlers call `_gameUi.PauseAndClear()` before
  returning so messages survive the next `PlayerLoop` `Console.Clear()`.
- **C0055 — AdminResetWorld:** Admin menu option `r` restores full world state (enemy HP, inventories,
  lock/trap states, Elara's starting gear) in one `SaveChanges()` call. Docs updated.

---

## 3. What I Used From the Template / AI / Other Sources

- **W15 template README.md:** Used as the starting structure for the
  W15 README. Grading rubric section and tier descriptions are from the
  template. The Design Deviations section and W15 implementation content
  are original.
- **W15 template CONTRIBUTIONS.md:** Used as the structural scaffold
  for this file. All content is original.
- **Claude (AI assistance):** Used extensively throughout the semester
  for architecture design, code review, migration hand-editing guidance,
  and implementation. The architecture decisions (bidirectional Door,
  ILockable LSP refactor, Container TPH extension pattern, MonsterLoot
  elimination) were planned collaboratively with AI assistance and then
  implemented in this repo. Code that I can walk through and explain
  end-to-end during the presentation.
- **No template implementation files used.** `MapManager.cs`,
  `ExplorationUI.cs`, `PlayerService.cs`, `AdminService.cs`,
  `ParserDemo.cs` — none of these were copied. See Design Deviations
  in README for the rationale and the equivalent methods in our
  `GameEngine.cs`.

---

## 4. Reflection on This Project

This project was built collaboratively with AI — Claude specifically —
and that was intentional. The course framed W15 as a capstone where the
expectation is that you understand what you built, not that you typed
every character yourself. I understand the architecture: what each class
is for, why the Container hierarchy is shaped the way it is, why
MonsterLoot was wrong, and what ILockable actually buys you across Chest,
Door, and LockedJournal. I can walk through a class and explain what it
does and why it exists. I cannot walk through a thousand lines of
GameEngine and recite it from memory, and I won't pretend otherwise.

What I did bring to this project was the design instinct. MonsterLoot
always bothered me — it felt like the world was organized around the
player taking things rather than around NPCs being real entities. When
we talked through the LucentForge bible and what it says about NPCs as
agents, the decision to remove MonsterLoot wasn't a technical call, it
was a design call. Same with Gobby — I didn't just want a renamed goblin,
I wanted two documents that tell the same story from opposite sides and
let neither one win. That kind of thing is mine.

What's next: this database is the foundation for LucentForge, a game I'm
actually building. The goal is an NPC runtime layer that drives behavior
using the Stats and Resources already in the schema. Phase E was the
first step toward that — an enemy who owns their inventory is an enemy
who can eventually decide what to do with it. The database is starting to
feel like a world, which is what I wanted it to be.

---

## 5. Course Feedback (NOT graded — please be candid)

*[Shawn fills this in — this section is not graded and should be in
your own voice. The AI should not author this section.]*

**What did you learn that genuinely stuck with you?**
[Your answer here]

**What did you like about the course?**
[Your answer here]

**What didn't work for you?**
[Your answer here]

**What surprised you?**
[Your answer here]

**What was the hardest part of the semester (not just this project)?**
[Your answer here]

**What would you ADD to next year's version?**
[Your answer here]

**What would you REMOVE or shorten?**
[Your answer here]

**Anything else?**
[Your answer here]

---

## How this is graded

**Sections 1-4** are the **gate to all rubric tiers.** Without a complete
and honest accounting of your starting point, additions, sources, and
project reflection, the project caps at 50% regardless of code quality.

- **Base/B/A/A+** all require Sections 1-4 to be filled out and to match
  what's actually in your repo.
- During your final presentation I may ask you to walk through any file
  you describe yourself as "added" or "modified" — be ready.
- Using template code with clear attribution is fine and earns full credit.
  Claiming to have written code you didn't is not, and will be graded
  as such (zero on the affected tier).

**Section 5 is not graded.** It exists to make the class better. A blank
Section 5 won't lower your grade; an honest critical Section 5 won't
either. The only "wrong" answer there is a fake one.

Think of Sections 1-4 as the README every PR needs: a short story about
what changed and why. It's a real engineering skill, and it's the most
reliable way for me to grade what you actually did.
