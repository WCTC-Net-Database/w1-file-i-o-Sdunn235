# W7 Mid-Term Prep — Phase Tracker

> **Naming Convention (Confirmed): Option B**
> - Solution: `w7-mid-term-prep.sln`
> - Projects: `ConsoleRpg` (UI + Game Logic) and `ConsoleRpgEntities` (Data + Models)
> - Namespaces: `ConsoleRpg` and `ConsoleRpgEntities`

---

## Phase 1 — Rename W6SolidDip → ConsoleRpg

**Goal:** Replace all traces of `W6SolidDip` and `w6-solid-dip` with the correct W7 identifiers. No structural changes yet.

| Task | Status |
|------|--------|
| Rename `w6-solid-dip.sln` → `w7-mid-term-prep.sln` | ✅ Complete |
| Rename `w6-solid-dip.csproj` → `ConsoleRpg.csproj` | ✅ Complete |
| Update `.sln` project reference (name + path) | ✅ Complete |
| Update `<RootNamespace>` and `<AssemblyName>` in `.csproj` | ✅ Complete |
| Bulk-replace all `namespace W6SolidDip` → `namespace ConsoleRpg` | ✅ Complete |
| Bulk-replace all `using W6SolidDip` → `using ConsoleRpg` | ✅ Complete |
| Remove leftover `w4-ocp.csproj.user` artifact | ✅ Complete |
| Build verify — clean, no errors | ✅ Complete |

**Status: ✅ Complete**

---

## Phase 1.5 — Folder Structure Fix (Interfaces Cleanup)

**Goal:** The `Interfaces\Characters\` subfolder contains 14 non-interface class files (abstract + concrete classes) whose namespaces already declare `ConsoleRpg.Models.Characters.*`. Move them to match. After the fix `Interfaces\` contains interfaces only.

> **Note:** No code changes required — namespaces are already correct. File moves only.
> **Why now:** Doing this before Phase 2 makes the two-project split cleaner — it will be obvious which files belong in `ConsoleRpgEntities` vs `ConsoleRpg`.

| Task | Status |
|------|--------|
| Move `Interfaces\Characters\Character.cs` → `Models\Characters\` | ✅ Complete |
| Move `Interfaces\Characters\CharacterBase.cs` → `Models\Characters\` | ✅ Complete |
| Move `Interfaces\Characters\Players\Player.cs` → `Models\Characters\Players\` | ✅ Complete |
| Move `Interfaces\Characters\Npcs\Npc.cs` → `Models\Characters\Npcs\` | ✅ Complete |
| Move `Interfaces\Characters\Npcs\Monsters\Monster.cs` → `Models\Characters\Npcs\Monsters\` | ✅ Complete |
| Move `Interfaces\Characters\Npcs\Monsters\Ghost.cs` → `Models\Characters\Npcs\Monsters\` | ✅ Complete |
| Move `Interfaces\Characters\Npcs\Monsters\Goblin.cs` → `Models\Characters\Npcs\Monsters\` | ✅ Complete |
| Move `Interfaces\Characters\Npcs\Monsters\Troll.cs` → `Models\Characters\Npcs\Monsters\` | ✅ Complete |
| Move `Interfaces\Characters\Npcs\Townspeople\Townsperson.cs` → `Models\Characters\Npcs\Townspeople\` | ✅ Complete |
| Move `Interfaces\Characters\Npcs\Townspeople\Races\*.cs` (5 files) → `Models\Characters\Npcs\Townspeople\Races\` | ✅ Complete |
| Remove now-empty `Interfaces\Characters\` folder tree | ✅ Complete |
| Build verify — clean, no errors | ✅ Complete |

> **Phase 2 Clean-up Note:** `BasicCharacter.cs`, `CharacterDto.cs`, `CharacterMapper.cs`, and `CharacterDtoMap.cs` are all scheduled for retirement when `IContext`/`GameContext` replaces the DTO→Mapper chain in Phase 2.

**Status: ✅ Complete**

---

## Phase 2 — Two-Project Architecture

**Goal:** Split the solution into `ConsoleRpg/` (UI + logic) and `ConsoleRpgEntities/` (data + models) per the README template structure.

| Task | Status |
|------|--------|
| Create `ConsoleRpgEntities/` folder and class library `.csproj` | ✅ Complete |
| Add `Data/` layer: `IContext.cs`, `IEntityDao.cs`, `GameContext.cs`, `PlayerDao.cs`, `MonsterDao.cs` | ✅ Complete |
| Add `Models/`: `Player.cs`, `MonsterBase.cs`, `Goblin.cs`, `Dragon.cs`, `Item.cs`, `AbilityScores.cs`, `Attribute.cs` | ✅ Complete |
| Add `Interfaces/IMonster.cs` | ✅ Complete |
| Add JSON data files: `players.json`, `monsters.json`, `items.json` | ✅ Complete |
| Move current project files into `ConsoleRpg/` subfolder | ✅ Complete |
| Add project reference `ConsoleRpg` → `ConsoleRpgEntities` in `.csproj` | ✅ Complete |
| Update `.sln` to reference both projects at correct paths | ✅ Complete |
| Build verify — both projects compile cleanly | ✅ Complete |

**Status: ✅ Complete**

---

## Phase 3 — New Components (ConsoleRpg)

**Goal:** Add all new services, UI, decorators, and helpers described in the README template. W6 GameEngine demo extracted to its own class and accessible via menu option (Option C).

| Task | Status |
|------|--------|
| `appsettings.json` + `Helpers/ConfigurationHelper.cs` | ✅ Complete |
| `Services/IBattleService.cs` + `Services/BattleService.cs` | ✅ Complete |
| `Services/IPlayerService.cs` + `Services/PlayerService.cs` | ✅ Complete |
| `Services/GameEngineDemo.cs` — W6 demo extracted from Program.cs (SRP) | ✅ Complete |
| `UI/IGameUi.cs` + `UI/ConsoleGameUi.cs` (includes W6 demo as menu option) | ✅ Complete |
| `Decorators/AutoSavePlayerServiceDecorator.cs` | ✅ Complete |
| `Startup.cs` — DI composition root | ✅ Complete |
| Update `GameEngine.cs` — add W7 constructor + `RunCombat()` + `ViewPlayer()` | ✅ Complete |
| Update `Program.cs` — remove inline demo, delegate to Startup + GameEngine | ✅ Complete |
| Build verify — full solution clean | ✅ Complete |

**Status: ✅ Complete**

---

## Phase 3.5 — Post-Phase 3 Fixes & Features

**Goal:** Runtime bugs discovered during testing, plus battle reset feature added.

### Battle Reset Feature
| Task | Status |
|------|--------|
| Add `bool AskResetBattle()` to `IGameUi` + `ConsoleGameUi` | ✅ Complete |
| Add `GameEngine.ResetBattle()` — reloads context + heals player to MaxHp | ✅ Complete |
| Update `GameEngine.RunCombat()` — auto-prompt reset on player death | ✅ Complete |
| Add menu option 3 "Reset Battle" to `ConsoleGameUi.GetMenuChoice()` | ✅ Complete |
| Shift Character Manager → option 4, W6 Demo → option 5 in `Program.cs` | ✅ Complete |

### Bug Fixes
| Bug | Root Cause | Fix | Status |
|-----|-----------|-----|--------|
| `ConsoleRpgEntities.Goblin` had `override Attack()` instead of `override PerformSpecialAction()` | Phase 2 file creation picked up W6 content | Corrected method override | ✅ Fixed |
| Battle reset not restoring monsters | `GameContext.SaveChanges()` was writing all three JSON files — damaged monster state overwrote `monsters.json` on every combat round | `SaveChanges()` now only writes `players.json` | ✅ Fixed |
| Reset still failing after code fix | Previous runs had already corrupted output `monsters.json`; `PreserveNewest` wouldn't overwrite it | Changed `monsters.json` and `items.json` to `CopyToOutputDirectory = Always` in `.csproj` | ✅ Fixed |

**Status: ✅ Complete**

---

## Phase 4 — Final Validation

**Goal:** Confirm the full solution matches the README template structure and runs correctly.

| Task | Status |
|------|--------|
| Verify folder structure matches README tree exactly | ✅ Complete |
| Run program end-to-end — no runtime errors | ✅ Complete |
| Confirm SOLID principles intact across all new/modified files | ✅ Complete |

**Status: ✅ Complete**

---

### Phase 4 — Findings

#### Structure vs README

| README Requires | We Have | Match |
|---|---|---|
| `ConsoleRpg.csproj` | ✅ `ConsoleRpg/ConsoleRpg.csproj` | ✅ |
| `Program.cs` | ✅ `ConsoleRpg/Program.cs` | ✅ |
| `Startup.cs` | ✅ `ConsoleRpg/Startup.cs` | ✅ |
| `GameEngine.cs` | ✅ `ConsoleRpg/Services/GameEngine.cs` | ✅ (in Services — better organized) |
| `appsettings.json` | ✅ `ConsoleRpg/appsettings.json` | ✅ |
| `Services/BattleService.cs` + `IBattleService.cs` | ✅ | ✅ |
| `Services/PlayerService.cs` + `IPlayerService.cs` | ✅ | ✅ |
| `UI/ConsoleGameUi.cs` + `IGameUi.cs` | ✅ | ✅ |
| `Decorators/AutoSavePlayerServiceDecorator.cs` | ✅ | ✅ |
| `Helpers/ConfigurationHelper.cs` | ✅ | ✅ |
| `ConsoleRpgEntities.csproj` | ✅ | ✅ |
| `Data/IContext.cs` | ✅ | ✅ |
| `Data/IEntityDao.cs` | ✅ | ✅ |
| `Data/GameContext.cs` | ✅ | ✅ |
| `Data/PlayerDao.cs` | ✅ | ✅ |
| `Data/MonsterDao.cs` | ✅ | ✅ |
| `Models/Player.cs` | ✅ | ✅ |
| `Models/MonsterBase.cs` | ✅ | ✅ |
| `Models/Goblin.cs` | ✅ | ✅ |
| `Models/Dragon.cs` | ✅ | ✅ |
| `Models/Item.cs` | ✅ | ✅ |
| `Models/AbilityScores.cs` | ✅ | ✅ |
| `Models/Attribute.cs` | ✅ | ✅ |
| `Interfaces/IMonster.cs` | ✅ | ✅ |
| `Files/players.json` | ✅ | ✅ |
| `Files/monsters.json` | ✅ | ✅ |
| `Files/items.json` | ✅ | ✅ |

**Additional files beyond README** (preserved W6 content + Option C):
`GameEngineDemo.cs`, `CharacterUI.cs`, `MenuService.cs`, `CsvFileHandler.cs`, `JsonFileHandler.cs`, `IFileHandler.cs`, `CharacterReader.cs`, `CharacterWriter.cs`, `Services/Commands/*`, full character model hierarchy, `Models/Classes/*`, `Models/DataTransfer/*`, `Models/Mapping/*`

#### SOLID Principles Verified

| Principle | Key Evidence |
|---|---|
| **SRP** | `Program.cs` is 50 lines, entry point only. `Startup.cs` is the only file that calls `new`. `BattleService` does math only. `ConsoleGameUi` does display only. |
| **OCP** | Adding a new monster = 1 new class + 1 attribute line + 1 JSON entry. Zero changes to existing services. |
| **LSP** | `AutoSavePlayerServiceDecorator` substitutes for `IPlayerService` without breaking callers. `Dragon`/`Goblin` substitute for `MonsterBase` anywhere. |
| **ISP** | `IContext`, `IEntityDao<T>`, `IBattleService`, `IPlayerService`, `IGameUi`, `IMonster` — each interface is focused and small. |
| **DIP** | `GameEngine` depends on 4 interfaces. `PlayerService` depends on `IContext` + `IEntityDao<T>`. `Startup.cs` is the only composition root. |

#### Fix Applied During Validation
`MonsterBase` now explicitly implements `IMonster` — it had all required properties but never declared the interface, leaving the ISP design incomplete.

---

## Legend
| Symbol | Meaning |
|--------|---------|
| ✅ | Complete |
| 🔄 | In Progress |
| ⬜ | Not Started |
| ❌ | Blocked / Issue |
