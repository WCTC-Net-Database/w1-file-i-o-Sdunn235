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

*[TODO: fill in after each phase ships — list every C0035+ commit here
with one sentence per item. Placeholder structure below.]*

- **Phase E — MonsterLoot eliminated:** Removed `MonsterLoot` as a
  separate Container subclass. NPCs now own their loot directly in their
  existing `Inventory` (Character already has `Inventory` from Phase 1.5).
  Migration `W15_KillMonsterLoot` moves items from MonsterLoot containers
  to their owning NPC's Inventory and drops the `LootId` / `IsLooted`
  columns.
- **Gobby the Outcast Goblin:** Renamed Grubnak to Gobby in seed data.
  Added narrative thread: Gobby is an outcast from his tribe, guarding
  the dungeon key that unlocks two doors further in.
- **LockedJournal — new ILockable item (F1):** `LockedJournal : Item,
  ILockable`. Exercises `TryUnlock` with zero changes to the method —
  the same algorithm that opens chests and doors also opens a locked
  journal. Seeded in the Hidden Shrine. Reading the journal surfaces
  Gobby's own account of why he was cast out.
- **Bookshelf + Tome — new Container + Item (F2):** `Bookshelf :
  Container` for information-holding containers. `Tome : Item` with
  `Title` and `LoreText`. Seeded in Ancient Library with tomes
  including the tribe's account of Gobby the Deserter — which
  contradicts the journal.
- **Wolf monster (F3):** `Wolf : Npc` (new Monster subtype) with
  `PackSize` property (a lone wolf is a scout; danger is in numbers).
  Seeded at Forest Edge. Proves Phase E works for a non-Gobby monster.
- **LINQ queries (Phase G):** InventoryAudit (GroupBy container type),
  MostDangerousRoom (GroupBy room, Sum health), LockedTreasures
  (all ILockable entities the player can't open — now spans Chests,
  Doors, AND LockedJournals via polymorphism).

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
