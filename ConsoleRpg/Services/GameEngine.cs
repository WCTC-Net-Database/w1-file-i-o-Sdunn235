using ConsoleRpg.UI;
using ConsoleRpgEntities.Data;
using ConsoleRpgEntities.Models;
using ConsoleRpgEntities.Models.Abilities;
using ConsoleRpgEntities.Models.Containers;
using ConsoleRpgEntities.Models.Enums;
using ConsoleRpgEntities.Models.Items;
using ConsoleRpgEntities.Models.Races;
using ConsoleRpgEntities.Models.Skills;
using MagicEntity = ConsoleRpgEntities.Models.Magic.Magic;

namespace ConsoleRpg.Services;

public class GameEngine
{
    private readonly IContext _dbContext;
    private readonly IGameUi _gameUi;

    // W12: active character concept — set via SelectCharacter, used by detail/inventory/equip.
    private Character? _activeCharacter;

    public GameEngine(IContext dbContext, IGameUi gameUi)
    {
        _dbContext = dbContext;
        _gameUi = gameUi;
    }

    public string? ActiveCharacterLabel =>
        _activeCharacter is null ? null : $"{_activeCharacter.Name} ({_activeCharacter.TypeName})";

    // Startup data-integrity sweep:
    //   1. Stats row must exist for every Character (default to all zeros).
    //   2. EquipmentSlots must only hold items whose EligibleSlot matches the slot.
    //      Bad rows from manual SQL inserts get nulled out so the equip pipeline is the
    //      only legal entry point.
    public void EnsureCharacterIntegrity()
    {
        // Materialize everything up-front. With lazy-loading proxies + a single
        // active DataReader, comparisons like `c.Stats == null` mid-enumeration
        // throw "There is already an open DataReader" — so we pull each set
        // fully into memory and compare via projected ID lookups instead.

        // 1. Backfill missing Stats rows.
        var characters = _dbContext.Characters.ToList();
        var statsCharacterIds = _dbContext.Stats.Select(s => s.CharacterId).ToHashSet();

        var orphans = characters.Where(c => !statsCharacterIds.Contains(c.Id)).ToList();
        foreach (var c in orphans)
            _dbContext.AddEntity(new Stats { CharacterId = c.Id });
        if (orphans.Count > 0)
            _dbContext.SaveChanges();

        // 2. Sweep EquipmentSlots for slot/item mismatches. Pull all items
        // first so we can resolve EquippedItemId without triggering proxy loads.
        var slots = _dbContext.EquipmentSlots
            .Where(s => s.EquippedItemId != null)
            .ToList();
        var itemsById = _dbContext.Items.ToList().ToDictionary(i => i.Id);

        int slotsCleared = 0;
        foreach (var slot in slots)
        {
            if (slot.EquippedItemId is not int itemId) continue;
            if (!itemsById.TryGetValue(itemId, out var item)) continue;

            // Shields are valid in either MainHand or OffHand.
            // Everything else: EligibleSlot must equal the slot's SlotType.
            bool valid;
            if (item is Shield)
                valid = slot.Slot == SlotType.MainHand || slot.Slot == SlotType.OffHand;
            else
                valid = item.EligibleSlot.HasValue && item.EligibleSlot.Value == slot.Slot;

            if (!valid)
            {
                slot.EquippedItemId = null;
                slotsCleared++;
            }
        }

        if (slotsCleared > 0) _dbContext.SaveChanges();

        // 3. Phase 1.5: every Character gets Inventory + Equipment containers.
        // Backfill for any pre-existing character (Player, NPC, Animal) that
        // doesn't have them. Re-fetch character list — IDs are stable but
        // EnsureContainersFor calls SaveChanges and we want a clean snapshot.
        int containersBackfilled = 0;
        foreach (var c in _dbContext.Characters.ToList())
        {
            if (EnsureContainersFor(c)) containersBackfilled++;
        }

        if (orphans.Count > 0 || slotsCleared > 0 || containersBackfilled > 0)
            Console.WriteLine($"[Startup] Integrity sweep: backfilled {orphans.Count} Stats row(s), cleared {slotsCleared} invalid equipment slot(s), backfilled containers for {containersBackfilled} character(s).");
    }

    // -------------------------------------------------------------------------
    // Character CRUD
    // -------------------------------------------------------------------------

    public void DisplayCharacters()
    {
        var characters = _dbContext.Characters.ToList();

        if (!characters.Any())
        {
            Console.WriteLine("\nNo characters found in the database.");
            return;
        }

        Console.WriteLine("\n=== Characters ===\n");
        foreach (var c in characters)
        {
            string raceLabel = c.Race?.Name ?? "No Race";
            string roomLabel = c.Room?.Name ?? "No Room";
            Console.WriteLine($"  [{c.Id}] {c.Name} ({c.TypeName}) (Lv {c.Level}) — {raceLabel} — Room: {roomLabel}");
        }
    }

    public void SelectCharacter()
    {
        Console.Write("Enter character name to select: ");
        var name = Console.ReadLine() ?? string.Empty;

        var character = _dbContext.Characters
            .FirstOrDefault(c => c.Name.Contains(name));

        if (character is null)
        {
            Console.WriteLine("\nCharacter not found.");
            return;
        }

        _activeCharacter = character;
        Console.WriteLine($"\nActive character set: [{character.Id}] {character.Name} ({character.TypeName}, Level {character.Level})");
    }

    // Back-compat shim in case anything still references FindCharacter.
    public void FindCharacter() => SelectCharacter();

    public void AddCharacter()
    {
        Console.Write("Character name: ");
        var name = Console.ReadLine() ?? string.Empty;

        Console.Write("Level: ");
        if (!int.TryParse(Console.ReadLine(), out var level))
        {
            Console.WriteLine("Invalid level.");
            return;
        }

        Console.WriteLine("\nCharacter type:");
        Console.WriteLine("  1. Player  (the hero character — playable race required)");
        Console.WriteLine("  2. NPC     (non-player character — shopkeeper, quest giver, enemy)");
        Console.WriteLine("  3. Animal  (wildlife — wolves, birds, etc.)");
        Console.Write("Choice: ");
        var typeChoice = Console.ReadLine()?.Trim();

        var races = _dbContext.Races.ToList();
        if (races.Any())
        {
            Console.WriteLine("\nAvailable Races:");
            foreach (var r in races)
                Console.WriteLine($"  [{r.Id}] {r.Name} ({r.GetType().BaseType?.Name ?? r.GetType().Name})");
        }

        Console.Write("Race ID (or blank for none): ");
        var raceInput = Console.ReadLine()?.Trim();
        int? raceId = null;
        Race? selectedRace = null;

        if (!string.IsNullOrEmpty(raceInput) && int.TryParse(raceInput, out var rid))
        {
            selectedRace = races.FirstOrDefault(r => r.Id == rid);
            if (selectedRace != null)
                raceId = rid;
        }

        var rooms = _dbContext.Rooms.ToList();
        if (rooms.Any())
        {
            Console.WriteLine("\nAvailable Rooms:");
            foreach (var r in rooms)
                Console.WriteLine($"  [{r.Id}] {r.Name}");
        }

        Console.Write("Room ID (or blank for none): ");
        var roomInput = Console.ReadLine()?.Trim();
        int? roomId = null;
        if (!string.IsNullOrEmpty(roomInput) && int.TryParse(roomInput, out var rmId))
        {
            if (rooms.Any(r => r.Id == rmId))
                roomId = rmId;
        }

        Character character;
        switch (typeChoice)
        {
            case "1":
                if (selectedRace != null && selectedRace is not PlayableRace)
                {
                    Console.WriteLine("Players can only be assigned a Playable race.");
                    return;
                }
                character = new Player { Name = name, Level = level, RoomId = roomId, RaceId = raceId };
                break;
            case "2":
                character = new Npc { Name = name, Level = level, RoomId = roomId, RaceId = raceId };
                break;
            case "3":
                character = new Animal { Name = name, Level = level, RoomId = roomId, RaceId = raceId };
                break;
            default:
                Console.WriteLine("Invalid type.");
                return;
        }

        _dbContext.AddEntity(character);
        _dbContext.SaveChanges();

        // Stats default to 0 — never null. Edit via Edit Character menu to raise them.
        var stats = new Stats
        {
            CharacterId = character.Id,
            Physique = 0, Reflexes = 0, Constitution = 0,
            Intellect = 0, Intuition = 0, Linguistic = 0, Luck = 0
        };
        _dbContext.AddEntity(stats);
        _dbContext.SaveChanges();
        character.Stats = stats;

        var resources = new Resources
        {
            CharacterId = character.Id,
            Hp = character.DeriveMaxHp(), MaxHp = character.DeriveMaxHp(),
            Sp = character.DeriveMaxSp(), MaxSp = character.DeriveMaxSp(),
            BitPool = character.DeriveMaxBitPool(), MaxBitPool = character.DeriveMaxBitPool(),
            BytePool = character.DeriveMaxBytePool(), MaxBytePool = character.DeriveMaxBytePool()
        };
        _dbContext.AddEntity(resources);
        _dbContext.SaveChanges();

        // Phase 1.5: every character gets empty Inventory + Equipment containers
        // and a full set of 7 EquipmentSlots — Player, NPC, and Animal alike.
        EnsureContainersFor(character);

        Console.WriteLine($"\n{character.TypeName} '{name}' created.");
    }

    // Backfill empty Inventory + Equipment + EquipmentSlots for any character that
    // doesn't already have them. Used at character creation and during the startup
    // integrity sweep. Saves immediately. Returns true if anything was created.
    private bool EnsureContainersFor(Character character)
    {
        bool changed = false;

        bool hasInventory = _dbContext.Containers
            .OfType<Inventory>()
            .Any(i => i.OwnerCharacterId == character.Id);
        if (!hasInventory)
        {
            var inv = new Inventory
            {
                Name = $"{character.Name}'s Pack",
                OwnerCharacterId = character.Id,
                MaxWeight = 100
            };
            _dbContext.AddEntity(inv);
            changed = true;
        }

        bool hasEquipment = _dbContext.Containers
            .OfType<Equipment>()
            .Any(e => e.OwnerCharacterId == character.Id);
        if (!hasEquipment)
        {
            var eq = new Equipment
            {
                Name = $"{character.Name}'s Gear",
                OwnerCharacterId = character.Id
            };
            _dbContext.AddEntity(eq);
            _dbContext.SaveChanges(); // need eq.Id before seeding slots
            changed = true;

            // One slot per SlotType enum value, all empty.
            foreach (SlotType slotType in Enum.GetValues<SlotType>())
            {
                _dbContext.AddEntity(new EquipmentSlot
                {
                    CharacterId = character.Id,
                    Slot = slotType,
                    EquippedItemId = null,
                    EquipmentContainerId = eq.Id
                });
            }
        }

        if (changed) _dbContext.SaveChanges();
        return changed;
    }

    public void LevelUpCharacter()
    {
        var character = ResolveActiveOrPrompt("level up");
        if (character is null) return;

        character.Level++;
        _dbContext.SaveChanges();
        Console.WriteLine($"\n{character.Name} is now level {character.Level}!");
    }

    // -------------------------------------------------------------------------
    // Edit Character — submenu over Identity / Stats / Abilities / Spells /
    // Equipment / Inventory. Each leaf saves changes after the user confirms.
    // -------------------------------------------------------------------------

    public void EditCharacter()
    {
        var character = ResolveActiveOrPrompt("edit");
        if (character is null) return;

        while (true)
        {
            Console.WriteLine($"\n=== Edit: {character.Name} ({character.TypeName}) ===");
            Console.WriteLine("  1. Identity (Name, Level, Race, Room)");
            Console.WriteLine("  2. Stats");
            Console.WriteLine("  3. Abilities");
            Console.WriteLine("  4. Spells / Magic");
            Console.WriteLine("  5. Equipment");
            Console.WriteLine("  6. Inventory");
            Console.WriteLine("  0. Done");
            Console.Write("Choice: ");
            var choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1": EditIdentity(character); break;
                case "2": EditStats(character); break;
                case "3": EditAbilities(character); break;
                case "4": EditSpells(character); break;
                case "5": EditEquipment(character); break;
                case "6": EditInventory(character); break;
                case "0": return;
                default: Console.WriteLine("Invalid choice."); break;
            }
        }
    }

    private void EditIdentity(Character c)
    {
        Console.Write($"\nName [{c.Name}] (blank to keep): ");
        var name = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(name)) c.Name = name.Trim();

        Console.Write($"Level [{c.Level}] (blank to keep): ");
        var lvl = Console.ReadLine();
        if (int.TryParse(lvl, out var newLvl)) c.Level = newLvl;

        var races = _dbContext.Races.ToList();
        if (races.Any())
        {
            Console.WriteLine("\nAvailable Races (blank=no change, 0=clear):");
            foreach (var r in races)
                Console.WriteLine($"  [{r.Id}] {r.Name}");
            Console.Write($"Race ID [current: {c.Race?.Name ?? "None"}]: ");
            var raceInput = Console.ReadLine()?.Trim();
            if (int.TryParse(raceInput, out var rid))
            {
                if (rid == 0) { c.RaceId = null; }
                else if (races.Any(r => r.Id == rid))
                {
                    if (c is Player && races.First(r => r.Id == rid) is not PlayableRace)
                        Console.WriteLine("Players require a Playable race — leaving unchanged.");
                    else c.RaceId = rid;
                }
            }
        }

        var rooms = _dbContext.Rooms.ToList();
        if (rooms.Any())
        {
            Console.WriteLine("\nAvailable Rooms (blank=no change, 0=clear):");
            foreach (var r in rooms)
                Console.WriteLine($"  [{r.Id}] {r.Name}");
            Console.Write($"Room ID [current: {c.Room?.Name ?? "None"}]: ");
            var roomInput = Console.ReadLine()?.Trim();
            if (int.TryParse(roomInput, out var rmId))
            {
                if (rmId == 0) c.RoomId = null;
                else if (rooms.Any(r => r.Id == rmId)) c.RoomId = rmId;
            }
        }

        _dbContext.SaveChanges();
        Console.WriteLine("Identity updated.");
    }

    private void EditStats(Character c)
    {
        if (c.Stats is null)
        {
            // Defensive — backfill should prevent this, but never trust assumed state.
            var fresh = new Stats { CharacterId = c.Id };
            _dbContext.AddEntity(fresh);
            _dbContext.SaveChanges();
            c.Stats = fresh;
        }

        var s = c.Stats;
        var stats = new (string Label, Func<int> Get, Action<int> Set)[]
        {
            ("Physique",     () => s.Physique,     v => s.Physique = v),
            ("Reflexes",     () => s.Reflexes,     v => s.Reflexes = v),
            ("Constitution", () => s.Constitution, v => s.Constitution = v),
            ("Intellect",    () => s.Intellect,    v => s.Intellect = v),
            ("Intuition",    () => s.Intuition,    v => s.Intuition = v),
            ("Linguistic",   () => s.Linguistic,   v => s.Linguistic = v),
            ("Luck",         () => s.Luck,         v => s.Luck = v),
        };

        while (true)
        {
            Console.WriteLine($"\n--- Stats: {c.Name} ---");
            for (int i = 0; i < stats.Length; i++)
                Console.WriteLine($"  {i + 1}. {stats[i].Label,-13} {stats[i].Get()}");
            Console.WriteLine("  0. Back");
            Console.Write("Pick stat to edit: ");
            var pick = Console.ReadLine()?.Trim();
            if (pick == "0") return;
            if (!int.TryParse(pick, out var idx) || idx < 1 || idx > stats.Length)
            {
                Console.WriteLine("Invalid."); continue;
            }

            Console.Write($"New {stats[idx - 1].Label} value: ");
            if (!int.TryParse(Console.ReadLine(), out var v) || v < 0)
            {
                Console.WriteLine("Stats must be non-negative integers."); continue;
            }
            stats[idx - 1].Set(v);
            _dbContext.SaveChanges();
            Console.WriteLine($"{stats[idx - 1].Label} = {v}.");
        }
    }

    private void EditAbilities(Character c)
    {
        while (true)
        {
            Console.WriteLine($"\n--- Abilities: {c.Name} ---");
            if (c.Abilities.Any())
                foreach (var a in c.Abilities)
                    Console.WriteLine($"  [{a.Id}] {a.Name} (Power {a.Power})");
            else
                Console.WriteLine("  (none)");

            Console.WriteLine("  1. Add ability   2. Remove ability   0. Back");
            Console.Write("Choice: ");
            var ch = Console.ReadLine()?.Trim();
            if (ch == "0") return;

            if (ch == "1")
            {
                var pool = _dbContext.Abilities.Where(a => !c.Abilities.Contains(a)).ToList();
                if (!pool.Any()) { Console.WriteLine("No abilities available to add."); continue; }
                Console.WriteLine("\nAvailable:");
                foreach (var a in pool) Console.WriteLine($"  [{a.Id}] {a.Name}");
                Console.Write("Ability ID: ");
                if (int.TryParse(Console.ReadLine(), out var aid))
                {
                    var picked = pool.FirstOrDefault(a => a.Id == aid);
                    if (picked != null) { c.Abilities.Add(picked); _dbContext.SaveChanges(); Console.WriteLine($"Added {picked.Name}."); }
                    else Console.WriteLine("Not in pool.");
                }
            }
            else if (ch == "2")
            {
                if (!c.Abilities.Any()) { Console.WriteLine("Nothing to remove."); continue; }
                Console.Write("Ability ID to remove: ");
                if (int.TryParse(Console.ReadLine(), out var aid))
                {
                    var picked = c.Abilities.FirstOrDefault(a => a.Id == aid);
                    if (picked != null) { c.Abilities.Remove(picked); _dbContext.SaveChanges(); Console.WriteLine($"Removed {picked.Name}."); }
                    else Console.WriteLine("Not on character.");
                }
            }
            else Console.WriteLine("Invalid.");
        }
    }

    private void EditSpells(Character c)
    {
        while (true)
        {
            Console.WriteLine($"\n--- Spells / Magic: {c.Name} ---");
            if (c.Magics.Any())
                foreach (var m in c.Magics)
                    Console.WriteLine($"  [{m.Id}] {m.Name} ({m.Element}, Power {m.Power})");
            else
                Console.WriteLine("  (none)");

            Console.WriteLine("  1. Add spell   2. Remove spell   0. Back");
            Console.Write("Choice: ");
            var ch = Console.ReadLine()?.Trim();
            if (ch == "0") return;

            if (ch == "1")
            {
                var pool = _dbContext.Magics.Where(m => !c.Magics.Contains(m)).ToList();
                if (!pool.Any()) { Console.WriteLine("No spells available."); continue; }
                Console.WriteLine("\nAvailable:");
                foreach (var m in pool) Console.WriteLine($"  [{m.Id}] {m.Name} ({m.Element})");
                Console.Write("Spell ID: ");
                if (int.TryParse(Console.ReadLine(), out var mid))
                {
                    var picked = pool.FirstOrDefault(m => m.Id == mid);
                    if (picked != null) { c.Magics.Add(picked); _dbContext.SaveChanges(); Console.WriteLine($"Added {picked.Name}."); }
                    else Console.WriteLine("Not in pool.");
                }
            }
            else if (ch == "2")
            {
                if (!c.Magics.Any()) { Console.WriteLine("Nothing to remove."); continue; }
                Console.Write("Spell ID to remove: ");
                if (int.TryParse(Console.ReadLine(), out var mid))
                {
                    var picked = c.Magics.FirstOrDefault(m => m.Id == mid);
                    if (picked != null) { c.Magics.Remove(picked); _dbContext.SaveChanges(); Console.WriteLine($"Removed {picked.Name}."); }
                    else Console.WriteLine("Not on character.");
                }
            }
            else Console.WriteLine("Invalid.");
        }
    }

    private void EditEquipment(Character c)
    {
        while (true)
        {
            Console.WriteLine($"\n--- Equipment: {c.Name} ---");
            if (c.EquipmentSlots.Any())
                foreach (var slot in c.EquipmentSlots)
                    Console.WriteLine($"  {slot.Slot,-9} {slot.EquippedItem?.Name ?? "(empty)"}");
            else
                Console.WriteLine("  (no slots)");

            Console.WriteLine("  1. Equip from inventory   2. Unequip   0. Back");
            Console.Write("Choice: ");
            var ch = Console.ReadLine()?.Trim();
            switch (ch)
            {
                case "1": InventoryEquip(c); break;
                case "2": InventoryUnequip(c); break;
                case "0": return;
                default: Console.WriteLine("Invalid."); break;
            }
        }
    }

    private void EditInventory(Character c)
    {
        if (c.Inventory is null)
        {
            Console.WriteLine($"\n{c.Name} has no inventory container.");
            return;
        }

        while (true)
        {
            var items = Items(c).ToList();
            Console.WriteLine($"\n--- Inventory: {c.Name} ({items.Count} items) ---");
            foreach (var i in items)
                Console.WriteLine($"  [{i.Id}] {i.Name} ({i.TypeNameForItem()})");

            Console.WriteLine("  1. Add item by ID   2. Remove item by ID   0. Back");
            Console.Write("Choice: ");
            var ch = Console.ReadLine()?.Trim();

            if (ch == "0") return;

            if (ch == "1")
            {
                var pool = _dbContext.Items.Where(i => i.ContainerId == null).ToList();
                if (!pool.Any()) { Console.WriteLine("No unowned items in the database."); continue; }
                Console.WriteLine("\nUnowned items:");
                foreach (var i in pool) Console.WriteLine($"  [{i.Id}] {i.Name} ({i.TypeNameForItem()}, {i.Weight} lbs)");
                Console.Write("Item ID: ");
                if (int.TryParse(Console.ReadLine(), out var iid))
                {
                    var picked = pool.FirstOrDefault(i => i.Id == iid);
                    if (picked != null) { picked.ContainerId = c.Inventory.Id; _dbContext.SaveChanges(); Console.WriteLine($"Added {picked.Name}."); }
                    else Console.WriteLine("Not unowned.");
                }
            }
            else if (ch == "2")
            {
                if (!items.Any()) { Console.WriteLine("Nothing to remove."); continue; }
                Console.Write("Item ID to remove from inventory (item stays in DB, becomes unowned): ");
                if (int.TryParse(Console.ReadLine(), out var iid))
                {
                    var picked = items.FirstOrDefault(i => i.Id == iid);
                    if (picked != null) { picked.ContainerId = null; _dbContext.SaveChanges(); Console.WriteLine($"Removed {picked.Name}."); }
                    else Console.WriteLine("Not in inventory.");
                }
            }
            else Console.WriteLine("Invalid.");
        }
    }

    // -------------------------------------------------------------------------
    // Delete Character
    // -------------------------------------------------------------------------

    public void DeleteCharacter()
    {
        DisplayCharacters();
        Console.Write("\nName of character to delete: ");
        var name = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(name)) { Console.WriteLine("Cancelled."); return; }

        var character = _dbContext.Characters.FirstOrDefault(c =>
            c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (character is null) { Console.WriteLine($"No character named '{name}'."); return; }

        Console.Write($"Type DELETE to confirm removal of {character.Name} ({character.TypeName}): ");
        if (!string.Equals(Console.ReadLine(), "DELETE", StringComparison.Ordinal))
        {
            Console.WriteLine("Cancelled."); return;
        }

        // Touch nav properties so EF's ClientCascade tracks them before removal.
        _ = character.Stats; _ = character.Resources;
        _ = character.Inventory; _ = character.Equipment;

        if (_activeCharacter?.Id == character.Id) _activeCharacter = null;

        _dbContext.RemoveEntity(character);
        _dbContext.SaveChanges();
        Console.WriteLine($"Deleted {character.Name}.");
    }

    public void DisplayCharacterDetail()
    {
        var character = ResolveActiveOrPrompt("view");
        if (character is null) return;

        Console.WriteLine($"\n=== {character.Name} ({character.TypeName}) ===");
        Console.WriteLine($"  Level: {character.Level}");
        Console.WriteLine($"  Race: {character.Race?.Name ?? "None"}");
        Console.WriteLine($"  Room: {character.Room?.Name ?? "None"}");

        if (character.Stats != null)
        {
            var s = character.Stats;
            Console.WriteLine($"\n  --- Stats ---");
            Console.WriteLine($"  Physique:     {s.Physique}");
            Console.WriteLine($"  Reflexes:     {s.Reflexes}");
            Console.WriteLine($"  Constitution: {s.Constitution}");
            Console.WriteLine($"  Intellect:    {s.Intellect}");
            Console.WriteLine($"  Intuition:    {s.Intuition}");
            Console.WriteLine($"  Linguistic:   {s.Linguistic}");
            Console.WriteLine($"  Luck:         {s.Luck}");
        }

        if (character.Resources != null)
        {
            var r = character.Resources;
            Console.WriteLine($"\n  --- Resources ---");
            int maxHp = character.DeriveMaxHp();
            int maxSp = character.DeriveMaxSp();
            int maxBit = character.DeriveMaxBitPool();
            int maxByte = character.DeriveMaxBytePool();
            Console.WriteLine($"  HP: {Math.Min(r.Hp, maxHp)}/{maxHp}");
            Console.WriteLine($"  SP: {Math.Min(r.Sp, maxSp)}/{maxSp}");
            Console.WriteLine($"  BitPool: {Math.Min(r.BitPool, maxBit)}/{maxBit}");
            Console.WriteLine($"  BytePool: {Math.Min(r.BytePool, maxByte)}/{maxByte}");
        }

        Console.WriteLine($"\n  Attack: {character.GetTotalAttack()} | Defense: {character.GetTotalDefense()}");

        if (character.Abilities.Any())
        {
            Console.WriteLine($"\n  --- Abilities ---");
            foreach (var a in character.Abilities)
                Console.WriteLine($"  {a.Name} (Power: {a.Power}, Cost: {a.StaminaCost} SP, Stat: {a.PrimaryStat})");
        }

        if (character.Magics.Any())
        {
            Console.WriteLine($"\n  --- Magic ---");
            foreach (var m in character.Magics)
                Console.WriteLine($"  {m.Name} ({m.Element}) Power: {m.Power}, BitPool: {m.BitPoolCost}, Bytes: {m.BytePoolCost}");
        }

        if (character.CharacterSkills.Any())
        {
            Console.WriteLine($"\n  --- Skills ---");
            foreach (var cs in character.CharacterSkills)
                Console.WriteLine($"  {cs.Skill.Name} — Proficiency: {cs.Proficiency} (Primary: {cs.Skill.PrimaryAttribute})");
        }

        if (character.EquipmentSlots.Any())
        {
            Console.WriteLine($"\n  --- Equipment ---");
            foreach (var slot in character.EquipmentSlots)
            {
                string itemLabel = slot.EquippedItem?.Name ?? "(empty)";
                Console.WriteLine($"  {slot.Slot}: {itemLabel}");
            }
        }
    }

    // Active-character resolver: returns the active character, prompting to select if none.
    private Character? ResolveActiveOrPrompt(string action)
    {
        if (_activeCharacter != null) return _activeCharacter;

        Console.WriteLine($"\nNo active character. Select one to {action}.");
        SelectCharacter();
        return _activeCharacter;
    }

    // -------------------------------------------------------------------------
    // Room & Navigation
    // -------------------------------------------------------------------------

    public void DisplayRooms()
    {
        var rooms = _dbContext.Rooms.ToList();
        if (!rooms.Any())
        {
            Console.WriteLine("\nNo rooms found.");
            return;
        }

        Console.WriteLine("\n=== Rooms ===\n");
        foreach (var r in rooms)
        {
            Console.WriteLine($"  [{r.Id}] {r.Name} — {r.Description}");

            if (r.Characters.Any())
            {
                foreach (var c in r.Characters)
                    Console.WriteLine($"      · {c.Name} ({c.TypeName}, Lv {c.Level})");
            }

            // Editor view: show ALL doors including undiscovered secrets.
            var allDoors = r.AllDoors.ToList();
            if (allDoors.Any())
            {
                foreach (var d in allDoors)
                {
                    var other = d.GetOtherRoom(r);
                    var flags = (d.IsLocked ? " [LOCKED]" : "")
                              + (d.IsTrapped && !d.TrapDisarmed ? " [TRAPPED]" : "")
                              + (d.IsSecret ? (d.IsDiscovered ? " [secret-found]" : " [SECRET]") : "");
                    Console.WriteLine($"      → {d.Name} → {other.Name}{flags}");
                }
            }
        }
    }

    public void AddRoom()
    {
        Console.Write("Room name: ");
        var name = Console.ReadLine() ?? string.Empty;

        Console.Write("Description: ");
        var desc = Console.ReadLine() ?? string.Empty;

        var room = new Room { Name = name, Description = desc };
        _dbContext.AddEntity(room);
        _dbContext.SaveChanges();

        Console.WriteLine($"\nRoom '{name}' created (ID: {room.Id}).");
    }

    public void AddDoor()
    {
        DisplayRooms();

        Console.Write("Room A ID: ");
        if (!int.TryParse(Console.ReadLine(), out var roomAId)) { Console.WriteLine("Invalid."); return; }

        Console.Write("Room B ID: ");
        if (!int.TryParse(Console.ReadLine(), out var roomBId)) { Console.WriteLine("Invalid."); return; }

        if (roomAId == roomBId) { Console.WriteLine("A door must connect two different rooms."); return; }

        Console.Write("Door name (e.g., 'Oak Door'): ");
        var name = Console.ReadLine() ?? string.Empty;

        Console.Write("Description: ");
        var desc = Console.ReadLine() ?? string.Empty;

        Console.Write("Locked? (y/n): ");
        bool locked = (Console.ReadLine()?.Trim().ToLower() ?? "") == "y";

        Console.Write("Secret? (y/n): ");
        bool secret = (Console.ReadLine()?.Trim().ToLower() ?? "") == "y";

        var door = new Door
        {
            Name = name,
            Description = desc,
            IsLocked = locked,
            IsSecret = secret,
            IsDiscovered = !secret, // secret doors start undiscovered; non-secret are always visible
            RoomAId = roomAId,
            RoomBId = roomBId
        };

        _dbContext.AddEntity(door);
        _dbContext.SaveChanges();

        Console.WriteLine($"\nDoor '{name}' connects Room {roomAId} ↔ Room {roomBId}.");
    }

    public void DisplayCurrentRoom()
    {
        var player = ResolveActiveOrPrompt("view current room for");
        if (player is null) return;
        if (player.Room is null) { Console.WriteLine($"\n{player.Name} is not in any room."); return; }

        var room = player.Room;
        Console.WriteLine($"\n=== {room.Name} ===");
        Console.WriteLine($"  {room.Description}");

        var others = room.Characters.Where(c => c.Id != player.Id).ToList();
        if (others.Any())
        {
            Console.WriteLine("\n  Characters here:");
            foreach (var c in others)
                Console.WriteLine($"    {c.Name} ({c.TypeName})");
        }

        // Player view: hide undiscovered secret doors.
        var visibleDoors = room.AllDoors.Where(d => d.IsVisible).ToList();
        if (visibleDoors.Any())
        {
            Console.WriteLine("\n  Exits:");
            for (int i = 0; i < visibleDoors.Count; i++)
            {
                var d = visibleDoors[i];
                var other = d.GetOtherRoom(room);
                var flags = (d.IsLocked ? " [LOCKED]" : "")
                          + (d.IsTrapped && !d.TrapDisarmed ? " [TRAPPED]" : "");
                Console.WriteLine($"    [{i + 1}] {d.Name} → {other.Name}{flags}");
            }
        }
        else
        {
            Console.WriteLine("\n  No visible exits.");
        }
    }

    public void MovePlayer()
    {
        var player = ResolveActiveOrPrompt("move");
        if (player is null) return;
        if (player.Room is null) { Console.WriteLine($"\n{player.Name} is not in any room."); return; }

        DisplayCurrentRoom();

        var visibleDoors = player.Room.AllDoors.Where(d => d.IsVisible).ToList();
        if (!visibleDoors.Any()) { Console.WriteLine("\nNo doors to take."); return; }

        Console.Write("\nDoor number to take (or 0 to cancel): ");
        if (!int.TryParse(Console.ReadLine(), out var choice) || choice < 1 || choice > visibleDoors.Count)
        {
            Console.WriteLine("Cancelled.");
            return;
        }

        var door = visibleDoors[choice - 1];

        if (door.IsLocked)
        {
            Console.WriteLine($"\nThe {door.Name} is locked!");
            return;
        }

        var destination = door.GetOtherRoom(player.Room);
        player.RoomId = destination.Id;
        _dbContext.SaveChanges();

        Console.WriteLine($"\n{player.Name} passes through the {door.Name} into {destination.Name}.");
        DisplayCurrentRoom();
    }

    // -------------------------------------------------------------------------
    // Items
    // -------------------------------------------------------------------------

    public void AddItem()
    {
        Console.Write("Item type (1=Weapon, 2=Armor, 3=Shield, 4=Consumable): ");
        var typeChoice = Console.ReadLine()?.Trim();

        Console.Write("Name: ");
        var name = Console.ReadLine() ?? string.Empty;

        Console.Write("Description: ");
        var desc = Console.ReadLine() ?? string.Empty;

        Console.Write("Value: ");
        int.TryParse(Console.ReadLine(), out var value);

        Console.Write("Weight: ");
        int.TryParse(Console.ReadLine(), out var weight);

        Item item;
        switch (typeChoice)
        {
            case "1":
                Console.Write("Attack Power: ");
                int.TryParse(Console.ReadLine(), out var atk);
                Console.Write("Weapon Type (Sword/Axe/Mace/Bow/Staff/Dagger/Spear): ");
                Enum.TryParse<WeaponType>(Console.ReadLine(), true, out var wpnType);
                Console.Write("Durability: ");
                int.TryParse(Console.ReadLine(), out var wDur);
                item = new Weapon
                {
                    Name = name, Description = desc, Value = value, Weight = weight,
                    AttackPower = atk, WeaponType = wpnType, Durability = wDur
                };
                break;
            case "2":
                Console.Write("Defense Rating: ");
                int.TryParse(Console.ReadLine(), out var def);
                Console.Write("Weight Class (Light/Medium/Heavy): ");
                Enum.TryParse<ArmorWeight>(Console.ReadLine(), true, out var armorWt);
                Console.Write("Body Slot (Head/Chest/Legs/Feet/Hands): ");
                Enum.TryParse<BodySlot>(Console.ReadLine(), true, out var bodySlot);
                Console.Write("Durability: ");
                int.TryParse(Console.ReadLine(), out var aDur);
                item = new Armor
                {
                    Name = name, Description = desc, Value = value, Weight = weight,
                    DefenseRating = def, WeightClass = armorWt, Slot = bodySlot, Durability = aDur
                };
                break;
            case "3":
                Console.Write("Defense Rating: ");
                int.TryParse(Console.ReadLine(), out var sDef);
                Console.Write("Weight Class (Light/Medium/Heavy): ");
                Enum.TryParse<ArmorWeight>(Console.ReadLine(), true, out var sArmorWt);
                Console.Write("Durability: ");
                int.TryParse(Console.ReadLine(), out var sDur);
                item = new Shield
                {
                    Name = name, Description = desc, Value = value, Weight = weight,
                    DefenseRating = sDef, WeightClass = sArmorWt,
                    Slot = BodySlot.Hands, // placeholder; Shield.EligibleSlot overrides this
                    Durability = sDur
                };
                break;
            case "4":
                // W14 Phase B: Effect is a typed enum. The picker enumerates
                // every defined ConsumableEffect so adding a new value to the
                // enum (e.g., a Cure/Buff effect later) updates this prompt
                // automatically — no menu drift.
                Console.WriteLine("Effect:");
                foreach (var ce in Enum.GetValues<ConsumableEffect>())
                {
                    var hint = ce switch
                    {
                        ConsumableEffect.None     => "no effect (key item / lockpick / non-effect)",
                        ConsumableEffect.Heal     => "restores HP, capped at MaxHp",
                        ConsumableEffect.Stamina  => "restores SP, capped at MaxSp",
                        ConsumableEffect.BitPool  => "restores BitPool, capped at MaxBitPool",
                        ConsumableEffect.BytePool => "restores BytePool, capped at MaxBytePool",
                        _ => string.Empty,
                    };
                    Console.WriteLine($"  {(int)ce}. {ce} — {hint}");
                }
                Console.Write("Choice [0]: ");
                var effect = int.TryParse(Console.ReadLine(), out var ec)
                          && Enum.IsDefined(typeof(ConsumableEffect), ec)
                    ? (ConsumableEffect)ec
                    : ConsumableEffect.None;

                Console.Write("Potency (magnitude added to the resource; ignored when Effect=None): ");
                int.TryParse(Console.ReadLine(), out var potency);
                item = new Consumable
                {
                    Name = name, Description = desc, Value = value, Weight = weight,
                    Effect = effect, Potency = potency
                };
                break;
            default:
                Console.WriteLine("Invalid type.");
                return;
        }

        _dbContext.AddEntity(item);
        _dbContext.SaveChanges();
        Console.WriteLine($"\n{item.TypeNameForItem()} '{name}' created.");
    }

    // -------------------------------------------------------------------------
    // W12 — Inventory Management (any character)
    // -------------------------------------------------------------------------

    public void InventoryMenu()
    {
        var player = ResolveActiveOrPrompt("manage inventory for");
        if (player is null) return;

        if (player.Inventory is null)
        {
            Console.WriteLine($"\n{player.Name} has no inventory container. Seed or create one first.");
            return;
        }

        while (true)
        {
            Console.WriteLine($"\n=== Inventory Management — {player.Name} ===");
            Console.WriteLine("  1. List items (with weight)");
            Console.WriteLine("  2. Search by name");
            Console.WriteLine("  3. Group by type");
            Console.WriteLine("  4. Sort items");
            Console.WriteLine("  5. Equip item from inventory");
            Console.WriteLine("  6. Unequip item");
            Console.WriteLine("  7. Use consumable");
            Console.WriteLine("  8. Strongest weapon (graded)");
            Console.WriteLine("  9. Total value + breakdown (graded)");
            Console.WriteLine("  0. Back to main menu");
            Console.Write("Choice: ");
            var choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1": InventoryList(player); break;
                case "2": InventorySearch(player); break;
                case "3": InventoryGroupByType(player); break;
                case "4": InventorySort(player); break;
                case "5": InventoryEquip(player); break;
                case "6": InventoryUnequip(player); break;
                case "7": InventoryUseConsumable(player); break;
                case "8": InventoryStrongestWeapon(player); break;
                case "9": InventoryTotalValueBreakdown(player); break;
                case "0": return;
                default: Console.WriteLine("Invalid choice."); break;
            }
        }
    }

    private static IEnumerable<Item> Items(Character p) =>
        p.Inventory?.ItemsCollection ?? Enumerable.Empty<Item>();

    private void InventoryList(Character player)
    {
        var items = Items(player).ToList();
        var max = player.Inventory!.MaxWeight;
        var cur = items.Sum(i => i.Weight);

        Console.WriteLine($"\n--- Inventory ({items.Count} items, {cur} / {max} lbs) ---");
        if (!items.Any()) { Console.WriteLine("  (empty)"); return; }

        foreach (var i in items)
            Console.WriteLine($"  [{i.Id}] {i.Name} — {i.TypeNameForItem()}, {i.Weight} lbs, {i.Value}g");
    }

    private void InventorySearch(Character player)
    {
        Console.Write("Search query: ");
        var q = Console.ReadLine() ?? string.Empty;

        var hits = Items(player)
            .Where(i => i.Name.Contains(q, StringComparison.OrdinalIgnoreCase))
            .ToList();

        Console.WriteLine($"\n--- Results for '{q}' ({hits.Count}) ---");
        if (!hits.Any()) { Console.WriteLine("  No matches."); return; }
        foreach (var i in hits)
            Console.WriteLine($"  [{i.Id}] {i.Name} — {i.TypeNameForItem()}");
    }

    private void InventoryGroupByType(Character player)
    {
        var groups = Items(player)
            .GroupBy(i => i.TypeNameForItem())
            .OrderBy(g => g.Key)
            .ToList();

        Console.WriteLine("\n--- Grouped by type ---");
        if (!groups.Any()) { Console.WriteLine("  (empty inventory)"); return; }

        foreach (var g in groups)
        {
            Console.WriteLine($"  {g.Key} ({g.Count()}):");
            foreach (var i in g)
                Console.WriteLine($"    [{i.Id}] {i.Name}");
        }
    }

    private void InventorySort(Character player)
    {
        Console.WriteLine("\nSort by: 1. Name  2. Value (desc)  3. Weight (asc)");
        Console.Write("Choice: ");
        var choice = Console.ReadLine()?.Trim();

        IEnumerable<Item> sorted = choice switch
        {
            "1" => Items(player).OrderBy(i => i.Name),
            "2" => Items(player).OrderByDescending(i => i.Value),
            "3" => Items(player).OrderBy(i => i.Weight),
            _ => Enumerable.Empty<Item>()
        };

        var list = sorted.ToList();
        if (!list.Any()) { Console.WriteLine("Nothing to show."); return; }

        Console.WriteLine();
        foreach (var i in list)
            Console.WriteLine($"  [{i.Id}] {i.Name} — {i.TypeNameForItem()}, {i.Weight} lbs, {i.Value}g");
    }

    private void InventoryEquip(Character player)
    {
        var equipables = Items(player).Where(i => i is Weapon or Armor).ToList();
        if (!equipables.Any()) { Console.WriteLine("\nNo weapons or armor in inventory."); return; }

        Console.WriteLine("\n--- Equipable items ---");
        foreach (var i in equipables)
            Console.WriteLine($"  [{i.Id}] {i.Name} ({i.TypeNameForItem()})");

        Console.Write("Item ID: ");
        if (!int.TryParse(Console.ReadLine(), out var id)) { Console.WriteLine("Invalid."); return; }

        var item = equipables.FirstOrDefault(i => i.Id == id);
        if (item is null) { Console.WriteLine("Not in equipables."); return; }

        if (player.Equip(item))
        {
            _dbContext.SaveChanges();
            Console.WriteLine($"Equipped {item.Name}.");
        }
        else Console.WriteLine("Could not equip (no compatible open slot, or no Equipment container).");
    }

    private void InventoryUnequip(Character player)
    {
        if (player.Equipment is null) { Console.WriteLine("\nNo Equipment container."); return; }
        var equipped = player.Equipment.ItemsCollection.ToList();
        if (!equipped.Any()) { Console.WriteLine("\nNothing equipped."); return; }

        Console.WriteLine("\n--- Equipped items ---");
        foreach (var i in equipped)
            Console.WriteLine($"  [{i.Id}] {i.Name} ({i.TypeNameForItem()})");

        Console.Write("Item ID to unequip: ");
        if (!int.TryParse(Console.ReadLine(), out var id)) { Console.WriteLine("Invalid."); return; }
        var item = equipped.FirstOrDefault(i => i.Id == id);
        if (item is null) { Console.WriteLine("Not equipped."); return; }

        player.Unequip(item);
        _dbContext.SaveChanges();
        Console.WriteLine($"Unequipped {item.Name}.");
    }

    private void InventoryUseConsumable(Character player)
    {
        var consumables = Items(player).OfType<Consumable>().ToList();
        if (!consumables.Any()) { Console.WriteLine("\nNo consumables."); return; }

        Console.WriteLine("\n--- Consumables ---");
        foreach (var c in consumables)
            Console.WriteLine($"  [{c.Id}] {c.Name} — {c.Effect}, potency {c.Potency}");

        Console.Write("Item ID: ");
        if (!int.TryParse(Console.ReadLine(), out var id)) { Console.WriteLine("Invalid."); return; }
        var item = consumables.FirstOrDefault(i => i.Id == id);
        if (item is null) { Console.WriteLine("Not found."); return; }

        player.UseItem(item);
        _dbContext.SaveChanges();
        Console.WriteLine($"Used {item.Name}.");
    }

    // ---- Graded LINQ Task A: Strongest Weapon ----
    private void InventoryStrongestWeapon(Character player)
    {
        var strongest = Items(player)
            .OfType<Weapon>()
            .OrderByDescending(w => w.AttackPower)
            .FirstOrDefault();

        Console.WriteLine("\n--- Strongest Weapon ---");
        if (strongest is null) Console.WriteLine("  No weapons in inventory.");
        else Console.WriteLine($"  {strongest.Name} — Attack {strongest.AttackPower} ({strongest.WeaponType})");
    }

    // ---- Graded LINQ Task B: Total Value + GroupBy breakdown ----
    private void InventoryTotalValueBreakdown(Character player)
    {
        var items = Items(player).ToList();

        int total = items.Sum(i => i.Value);

        var breakdown = items
            .GroupBy(i => i.TypeNameForItem())
            .Select(g => new { Type = g.Key, Gold = g.Sum(i => i.Value), Count = g.Count() })
            .OrderByDescending(x => x.Gold)
            .ToList();

        Console.WriteLine($"\n--- Inventory Value ---");
        Console.WriteLine($"  Total: {total}g across {items.Count} items");
        if (!breakdown.Any()) return;

        Console.WriteLine("  By type:");
        foreach (var b in breakdown)
            Console.WriteLine($"    {b.Type,-12} {b.Count,3} items   {b.Gold,6}g");
    }

    // -------------------------------------------------------------------------
    // W13 — Chest & Monster Loot Interaction (any character)
    // -------------------------------------------------------------------------

    public void ChestMenu()
    {
        var player = ResolveActiveOrPrompt("interact with chests for");
        if (player is null) return;

        while (true)
        {
            Console.WriteLine($"\n=== Chest Interaction — {player.Name} ===");
            Console.WriteLine("  1. List chests in current room");
            Console.WriteLine("  2. Open chest");
            Console.WriteLine("  3. Try unlock chest (key or lockpick)");
            Console.WriteLine("  4. Disarm trap (lockpick) — graded");
            Console.WriteLine("  5. Loot chest");
            Console.WriteLine("  6. Loot defeated monster");
            Console.WriteLine("  7. Richest locked chest (graded)");
            Console.WriteLine("  0. Back to main menu");
            Console.Write("Choice: ");
            var choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1": ChestList(player); break;
                case "2": ChestOpen(player); break;
                case "3": ChestTryUnlock(player); break;
                case "4": ChestDisarmTrap(player); break;
                case "5": ChestLoot(player); break;
                case "6": ChestLootMonster(player); break;
                case "7": ChestRichestLocked(); break;
                case "0": return;
                default: Console.WriteLine("Invalid choice."); break;
            }
        }
    }

    private List<Chest> ChestsInCharacterRoom(Character player) =>
        _dbContext.Containers
            .OfType<Chest>()
            .Where(c => c.RoomId == player.RoomId)
            .ToList();

    private void ChestList(Character player)
    {
        if (player.RoomId is null) { Console.WriteLine("\nPlayer is not in a room."); return; }

        var chests = ChestsInCharacterRoom(player);
        Console.WriteLine($"\n--- Chests in {player.Room?.Name ?? "this room"} ({chests.Count}) ---");
        if (!chests.Any()) { Console.WriteLine("  (none)"); return; }

        foreach (var c in chests)
        {
            string state = (c.IsLocked, c.IsTrapped && !c.TrapDisarmed) switch
            {
                (true, true)   => "[LOCKED, TRAPPED]",
                (true, false)  => "[LOCKED]",
                (false, true)  => "[TRAPPED]",
                _              => "[OPEN]"
            };
            Console.WriteLine($"  [{c.Id}] {c.Name} {state} — {c.Description}");
        }
    }

    private Chest? PromptForChest(Character player)
    {
        var chests = ChestsInCharacterRoom(player);
        if (!chests.Any()) { Console.WriteLine("\nNo chests in this room."); return null; }

        Console.Write("Chest ID: ");
        if (!int.TryParse(Console.ReadLine(), out var id)) { Console.WriteLine("Invalid."); return null; }
        var chest = chests.FirstOrDefault(c => c.Id == id);
        if (chest is null) Console.WriteLine("Not in this room.");
        return chest;
    }

    private void ChestOpen(Character player)
    {
        var chest = PromptForChest(player);
        if (chest is null) return;

        var result = player.OpenChest(chest);
        switch (result)
        {
            case OpenResult.Opened:
                Console.WriteLine($"\nThe {chest.Name} opens. {chest.ItemsCollection.Count} item(s) inside.");
                break;
            case OpenResult.Locked:
                Console.WriteLine($"\nThe {chest.Name} is locked. Try option 3 to unlock.");
                break;
            case OpenResult.Trapped:
                Console.WriteLine($"\nA trap fires! {chest.Name} deals {chest.TrapDamage} damage. " +
                                  $"HP now {player.Resources?.Hp ?? 0}/{player.Resources?.MaxHp ?? 0}.");
                _dbContext.SaveChanges();
                break;
            case OpenResult.AlreadyOpen:
                Console.WriteLine($"\nThe {chest.Name} was already open.");
                break;
        }
    }

    private void ChestTryUnlock(Character player)
    {
        var chest = PromptForChest(player);
        if (chest is null) return;
        if (!chest.IsLocked) { Console.WriteLine("\nNot locked."); return; }
        if (player.Inventory is null) { Console.WriteLine("\nNo inventory."); return; }

        var keys = player.Inventory.ItemsCollection.Where(i => i.IsKeyItem).ToList();
        if (!keys.Any()) { Console.WriteLine("\nNo keys or lockpicks in inventory."); return; }

        Console.WriteLine("\n--- Keys & Lockpicks ---");
        foreach (var k in keys)
        {
            string label = k.KeyId is null ? "(lockpick)" : $"(key: {k.KeyId})";
            Console.WriteLine($"  [{k.Id}] {k.Name} {label}");
        }

        Console.Write("Item ID: ");
        if (!int.TryParse(Console.ReadLine(), out var id)) { Console.WriteLine("Invalid."); return; }
        var key = keys.FirstOrDefault(k => k.Id == id);
        if (key is null) { Console.WriteLine("Not in keys list."); return; }

        bool ok = player.TryUnlock(chest, key);
        _dbContext.SaveChanges();
        if (ok)
            Console.WriteLine($"\n{chest.Name} clicks open.");
        else if (key.KeyId is null)
            Console.WriteLine($"\nThe lockpick snaps. {chest.Name} stays shut.");
        else
            Console.WriteLine($"\nThat key doesn't fit {chest.Name}.");
    }

    private void ChestDisarmTrap(Character player)
    {
        var chest = PromptForChest(player);
        if (chest is null) return;
        if (player.Inventory is null) { Console.WriteLine("\nNo inventory."); return; }

        var lockpick = player.Inventory.ItemsCollection
            .FirstOrDefault(i => i.IsKeyItem && i.KeyId is null);
        if (lockpick is null) { Console.WriteLine("\nNo lockpick available."); return; }

        bool ok = player.DisarmTrap(chest, lockpick);
        _dbContext.SaveChanges();
        Console.WriteLine(ok
            ? $"\nTrap on {chest.Name} disarmed. Lockpick used."
            : $"\nCouldn't disarm — chest isn't trapped, or already disarmed, or item isn't a lockpick.");
    }

    private void ChestLoot(Character player)
    {
        var chest = PromptForChest(player);
        if (chest is null) return;
        if (chest.IsLocked) { Console.WriteLine("\nLocked. Unlock first."); return; }

        LootInteractive(player, chest, $"Inside {chest.Name}");
        _dbContext.SaveChanges();
    }

    private void ChestLootMonster(Character player)
    {
        if (player.RoomId is null) { Console.WriteLine("\nPlayer is not in a room."); return; }

        var monsters = _dbContext.Characters
            .OfType<Npc>()
            .Where(n => n.RoomId == player.RoomId && n.LootId != null)
            .ToList();

        if (!monsters.Any()) { Console.WriteLine("\nNo defeated monsters here to loot."); return; }

        Console.WriteLine("\n--- Defeated monsters here ---");
        foreach (var m in monsters)
        {
            string status = m.Loot?.IsLooted == true ? "(already looted)" : "(unlooted)";
            Console.WriteLine($"  [{m.Id}] {m.Name} ({m.Race?.Name}) {status}");
        }

        Console.Write("Monster ID: ");
        if (!int.TryParse(Console.ReadLine(), out var id)) { Console.WriteLine("Invalid."); return; }
        var monster = monsters.FirstOrDefault(m => m.Id == id);
        if (monster is null) { Console.WriteLine("Not in this room."); return; }

        if (monster.Loot is null) { Console.WriteLine("\nNothing to loot."); return; }

        LootInteractive(player, monster.Loot, $"On {monster.Name}'s body");
        // Mark as searched once the player exits the picker — the body's been
        // processed, even if items were left behind because the looter
        // couldn't carry them.
        monster.Loot.IsLooted = true;
        _dbContext.SaveChanges();
    }

    /// <summary>
    /// Interactive loot picker. Shows the source container's items in a
    /// numbered list with weight + value tags; player picks individual
    /// items by number, types "all" to grab everything that fits, or 0
    /// to leave. Re-displays after each take so the player sees the
    /// updated state. Items the looter can't fit show a [too heavy] tag
    /// and are skipped on "all".
    ///
    /// Reusable across chests, monster loot, room floors (W14), and any
    /// future Container subclass — the picker only knows about
    /// <see cref="Container"/> and <see cref="Character.TakeItemFrom"/>.
    /// </summary>
    private static void LootInteractive(Character looter, Container source, string sourceLabel)
    {
        if (looter.Inventory is null)
        {
            Console.WriteLine("\nThis character has no inventory to loot into.");
            return;
        }

        while (true)
        {
            var items = source.ItemsCollection.ToList();
            if (!items.Any())
            {
                Console.WriteLine($"\n{sourceLabel}: empty.");
                return;
            }

            Console.WriteLine($"\n--- {sourceLabel} ---");
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                string fitsTag = looter.Inventory.CanFit(item.Weight) ? "" : "  [too heavy]";
                Console.WriteLine(
                    $"  [{i + 1}] {item.Name} — {item.TypeNameForItem()}, " +
                    $"{item.Weight} lb, {item.Value}g{fitsTag}");
            }
            Console.Write("Take which? (number / 'all' / 0 to leave): ");
            var input = Console.ReadLine()?.Trim().ToLowerInvariant() ?? "";

            if (input == "0" || input == "")
                return;

            if (input == "all")
            {
                var taken = new List<Item>();
                foreach (var item in items)
                {
                    if (looter.TakeItemFrom(source, item))
                        taken.Add(item);
                }
                if (taken.Count > 0)
                {
                    Console.WriteLine($"\nTook {taken.Count} item(s):");
                    foreach (var t in taken)
                        Console.WriteLine($"  - {t.Name}");
                }
                else
                {
                    Console.WriteLine("\nCouldn't take anything (weight?).");
                }
                continue;
            }

            if (int.TryParse(input, out var idx) && idx >= 1 && idx <= items.Count)
            {
                var picked = items[idx - 1];
                if (looter.TakeItemFrom(source, picked))
                    Console.WriteLine($"\nTook {picked.Name}.");
                else
                    Console.WriteLine($"\nCouldn't take {picked.Name} — too heavy.");
                continue;
            }

            Console.WriteLine("Invalid choice.");
        }
    }

    // ---- Graded LINQ Task A: Richest locked chest ----
    private void ChestRichestLocked()
    {
        var richest = _dbContext.Containers
            .OfType<Chest>()
            .Where(c => c.IsLocked)
            .OrderByDescending(c => c.ItemsCollection.Sum(i => i.Value))
            .FirstOrDefault();

        Console.WriteLine("\n--- Richest Locked Chest ---");
        if (richest is null) { Console.WriteLine("  No locked chests."); return; }

        int gold = richest.ItemsCollection.Sum(i => i.Value);
        Console.WriteLine($"  {richest.Name} — {gold}g across {richest.ItemsCollection.Count} item(s)");
        Console.WriteLine($"  ({richest.Description})");
    }

    // -------------------------------------------------------------------------
    // Phase 1 reshape — thematic top-level submenus.
    // Each Submenu() loops until the user picks 0 (Back).
    // -------------------------------------------------------------------------

    public void CharactersSubmenu()
    {
        while (true)
        {
            Console.WriteLine("\n=== Characters ===");
            Console.WriteLine("  1. List characters");
            Console.WriteLine("  2. Select active character");
            Console.WriteLine("  3. Add character");
            Console.WriteLine("  4. Edit character");
            Console.WriteLine("  5. Delete character");
            Console.WriteLine("  6. Level up character");
            Console.WriteLine("  7. Character detail");
            Console.WriteLine("  0. Back");
            Console.Write("Choice: ");
            switch (Console.ReadLine()?.Trim())
            {
                case "1": DisplayCharacters(); break;
                case "2": SelectCharacter(); break;
                case "3": AddCharacter(); break;
                case "4": EditCharacter(); break;
                case "5": DeleteCharacter(); break;
                case "6": LevelUpCharacter(); break;
                case "7": DisplayCharacterDetail(); break;
                case "0": return;
                default: Console.WriteLine("Invalid."); break;
            }
        }
    }

    // -------------------------------------------------------------------------
    // Items submenu — list / create (subtype-aware) / view / edit / remove
    // -------------------------------------------------------------------------

    public void ItemsSubmenu()
    {
        while (true)
        {
            Console.WriteLine("\n=== Items ===");
            Console.WriteLine("  1. List items");
            Console.WriteLine("  2. Create item");
            Console.WriteLine("  3. View item detail");
            Console.WriteLine("  4. Edit item (common fields)");
            Console.WriteLine("  5. Remove item");
            Console.WriteLine("  0. Back");
            Console.Write("Choice: ");
            switch (Console.ReadLine()?.Trim())
            {
                case "1": ListItems(); break;
                case "2": AddItem(); break;
                case "3": ViewItemDetail(); break;
                case "4": EditItem(); break;
                case "5": RemoveItem(); break;
                case "0": return;
                default: Console.WriteLine("Invalid."); break;
            }
        }
    }

    private void ListItems()
    {
        var items = _dbContext.Items.ToList();
        if (!items.Any()) { Console.WriteLine("\nNo items in database."); return; }
        Console.WriteLine("\n--- Items ---");
        foreach (var i in items.OrderBy(i => i.TypeNameForItem()).ThenBy(i => i.Name))
        {
            // Lazy-loading proxies materialize i.Container on access. Show the
            // container's display name (Inventory: "Elara's Pack", Chest:
            // "Wooden Chest", Room: "Antechamber", etc.) under a single
            // "Owner" label per Shawn's call — works whether the container is
            // a person, a vessel, a room, or any future thing-that-holds-items.
            string ownerLabel = i.Container is { } c
                ? $"Owner: {c.Name}"
                : "Unowned";
            Console.WriteLine($"  [{i.Id}] {i.Name} — {i.TypeNameForItem()}, {i.Weight} lbs, {i.Value}g — {ownerLabel}");
        }
    }

    private Item? PromptItem(string verb)
    {
        ListItems();
        Console.Write($"\nItem ID to {verb}: ");
        if (!int.TryParse(Console.ReadLine(), out var id)) { Console.WriteLine("Invalid."); return null; }
        var item = _dbContext.Items.FirstOrDefault(i => i.Id == id);
        if (item is null) Console.WriteLine("Not found.");
        return item;
    }

    private void ViewItemDetail()
    {
        var item = PromptItem("view");
        if (item is null) return;
        Console.WriteLine($"\n=== [{item.Id}] {item.Name} ({item.TypeNameForItem()}) ===");
        Console.WriteLine($"  Description: {item.Description}");
        Console.WriteLine($"  Value: {item.Value}g   Weight: {item.Weight} lbs");
        Console.WriteLine($"  KeyItem: {item.IsKeyItem}   KeyId: {item.KeyId ?? "—"}");
        Console.WriteLine($"  EligibleSlot: {(item.EligibleSlot.HasValue ? item.EligibleSlot.ToString() : "—")}");
        switch (item)
        {
            case Weapon w: Console.WriteLine($"  Weapon: {w.WeaponType}, AP {w.AttackPower}, Dur {w.Durability}"); break;
            case Shield s: Console.WriteLine($"  Shield: {s.WeightClass}, DR {s.DefenseRating}, Dur {s.Durability}"); break;
            case Armor a: Console.WriteLine($"  Armor: {a.WeightClass} {a.Slot}, DR {a.DefenseRating}, Dur {a.Durability}"); break;
            case Consumable c: Console.WriteLine($"  Consumable: effect {c.Effect}, potency {c.Potency}"); break;
        }
    }

    private void EditItem()
    {
        var item = PromptItem("edit");
        if (item is null) return;

        Console.Write($"Name [{item.Name}] (blank to keep): ");
        var n = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(n)) item.Name = n.Trim();

        Console.Write($"Description [{item.Description}] (blank to keep): ");
        var d = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(d)) item.Description = d.Trim();

        Console.Write($"Value [{item.Value}] (blank to keep): ");
        if (int.TryParse(Console.ReadLine(), out var v)) item.Value = v;

        Console.Write($"Weight [{item.Weight}] (blank to keep): ");
        if (int.TryParse(Console.ReadLine(), out var w)) item.Weight = w;

        _dbContext.SaveChanges();
        Console.WriteLine("Item updated.");
    }

    private void RemoveItem()
    {
        var item = PromptItem("remove");
        if (item is null) return;

        Console.Write("(d)elete from database, (u)nown only, or (c)ancel? ");
        switch (Console.ReadLine()?.Trim().ToLower())
        {
            case "d":
                _dbContext.RemoveEntity(item);
                _dbContext.SaveChanges();
                Console.WriteLine("Item deleted.");
                break;
            case "u":
                item.ContainerId = null;
                _dbContext.SaveChanges();
                Console.WriteLine("Item is now unowned.");
                break;
            default:
                Console.WriteLine("Cancelled.");
                break;
        }
    }

    // -------------------------------------------------------------------------
    // Rooms & Doors submenu
    // Per design: room delete cascades doors and orphans (a) characters' RoomId,
    // (b) chests' RoomId, (c) items inside those chests' ContainerId.
    // Items themselves are preserved in the database.
    // -------------------------------------------------------------------------

    public void RoomsSubmenu()
    {
        while (true)
        {
            Console.WriteLine("\n=== Rooms & Doors ===");
            Console.WriteLine("  1. List rooms");
            Console.WriteLine("  2. Create room");
            Console.WriteLine("  3. Edit room");
            Console.WriteLine("  4. Remove room");
            Console.WriteLine("  5. Door management ▶");
            Console.WriteLine("  6. View current room (active player)");
            Console.WriteLine("  7. Move active player");
            Console.WriteLine("  0. Back");
            Console.Write("Choice: ");
            switch (Console.ReadLine()?.Trim())
            {
                case "1": DisplayRooms(); break;
                case "2": AddRoom(); break;
                case "3": EditRoom(); break;
                case "4": RemoveRoom(); break;
                case "5": DoorsSubmenu(); break;
                case "6": DisplayCurrentRoom(); break;
                case "7": MovePlayer(); break;
                case "0": return;
                default: Console.WriteLine("Invalid."); break;
            }
        }
    }

    private Room? PromptRoom(string verb)
    {
        DisplayRooms();
        Console.Write($"\nRoom ID to {verb}: ");
        if (!int.TryParse(Console.ReadLine(), out var id)) { Console.WriteLine("Invalid."); return null; }
        var room = _dbContext.Rooms.FirstOrDefault(r => r.Id == id);
        if (room is null) Console.WriteLine("Not found.");
        return room;
    }

    private void EditRoom()
    {
        var room = PromptRoom("edit");
        if (room is null) return;

        Console.Write($"Name [{room.Name}] (blank to keep): ");
        var n = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(n)) room.Name = n.Trim();

        Console.Write($"Description [{room.Description}] (blank to keep): ");
        var d = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(d)) room.Description = d.Trim();

        _dbContext.SaveChanges();
        Console.WriteLine("Room updated.");
    }

    private void RemoveRoom()
    {
        var room = PromptRoom("remove");
        if (room is null) return;

        // Snapshot dependents before mutation.
        var chestsHere = _dbContext.Containers.OfType<Chest>().Where(c => c.RoomId == room.Id).ToList();
        var doorsTouching = _dbContext.Doors
            .Where(d => d.RoomAId == room.Id || d.RoomBId == room.Id)
            .ToList();
        var charactersHere = _dbContext.Characters.Where(c => c.RoomId == room.Id).ToList();

        Console.WriteLine($"\nRemoving '{room.Name}' will:");
        Console.WriteLine($"  - orphan {charactersHere.Count} character(s) (RoomId cleared)");
        Console.WriteLine($"  - delete {chestsHere.Count} chest(s) in the room (their items become unowned)");
        Console.WriteLine($"  - delete {doorsTouching.Count} door(s) touching the room");
        Console.Write("Confirm? (y/N): ");
        if ((Console.ReadLine()?.Trim().ToLower() ?? "") != "y") { Console.WriteLine("Cancelled."); return; }

        // Orphan items inside chests, then delete chests.
        foreach (var chest in chestsHere)
        {
            var items = _dbContext.Items.Where(i => i.ContainerId == chest.Id).ToList();
            foreach (var i in items) i.ContainerId = null;
            _dbContext.RemoveEntity(chest);
        }
        // Orphan characters.
        foreach (var c in charactersHere) c.RoomId = null;
        // Delete doors.
        foreach (var d in doorsTouching) _dbContext.RemoveEntity(d);

        _dbContext.RemoveEntity(room);
        _dbContext.SaveChanges();
        Console.WriteLine($"Room '{room.Name}' removed.");
    }

    public void DoorsSubmenu()
    {
        while (true)
        {
            Console.WriteLine("\n=== Doors ===");
            Console.WriteLine("  1. List doors");
            Console.WriteLine("  2. Add door");
            Console.WriteLine("  3. Toggle lock on door");
            Console.WriteLine("  4. Remove door");
            Console.WriteLine("  0. Back");
            Console.Write("Choice: ");
            switch (Console.ReadLine()?.Trim())
            {
                case "1": ListDoors(); break;
                case "2": AddDoor(); break;
                case "3": ToggleDoorLock(); break;
                case "4": RemoveDoor(); break;
                case "0": return;
                default: Console.WriteLine("Invalid."); break;
            }
        }
    }

    private void ListDoors()
    {
        var doors = _dbContext.Doors.ToList();
        if (!doors.Any()) { Console.WriteLine("\nNo doors."); return; }
        Console.WriteLine("\n--- Doors ---");
        foreach (var d in doors)
        {
            var flags = (d.IsLocked ? " [LOCKED]" : "")
                      + (d.IsTrapped && !d.TrapDisarmed ? " [TRAPPED]" : "")
                      + (d.IsSecret ? (d.IsDiscovered ? " [secret-found]" : " [SECRET]") : "");
            Console.WriteLine($"  [{d.Id}] {d.Name}: {d.RoomA.Name} ↔ {d.RoomB.Name}{flags}");
        }
    }

    private Door? PromptDoor(string verb)
    {
        ListDoors();
        Console.Write($"\nDoor ID to {verb}: ");
        if (!int.TryParse(Console.ReadLine(), out var id)) { Console.WriteLine("Invalid."); return null; }
        var door = _dbContext.Doors.FirstOrDefault(d => d.Id == id);
        if (door is null) Console.WriteLine("Not found.");
        return door;
    }

    private void ToggleDoorLock()
    {
        var door = PromptDoor("toggle");
        if (door is null) return;
        door.IsLocked = !door.IsLocked;
        _dbContext.SaveChanges();
        Console.WriteLine($"Door '{door.Name}' is now {(door.IsLocked ? "LOCKED" : "unlocked")}.");
    }

    private void RemoveDoor()
    {
        var door = PromptDoor("remove");
        if (door is null) return;
        _dbContext.RemoveEntity(door);
        _dbContext.SaveChanges();
        Console.WriteLine($"Door '{door.Name}' removed.");
    }

    // -------------------------------------------------------------------------
    // Skills / Abilities / Magic — global definition CRUD + assign-to-character
    // -------------------------------------------------------------------------

    public void SkillsSubmenu()
    {
        while (true)
        {
            Console.WriteLine("\n=== Skills ===");
            Console.WriteLine("  1. List   2. Create   3. Edit   4. Delete   5. Assign to character   0. Back");
            Console.Write("Choice: ");
            switch (Console.ReadLine()?.Trim())
            {
                case "1": ListSkills(); break;
                case "2": CreateSkill(); break;
                case "3": EditSkillDef(); break;
                case "4": DeleteSkillDef(); break;
                case "5": AssignSkillToCharacter(); break;
                case "0": return;
                default: Console.WriteLine("Invalid."); break;
            }
        }
    }

    private void ListSkills()
    {
        var skills = _dbContext.Skills.ToList();
        if (!skills.Any()) { Console.WriteLine("\nNo skills."); return; }
        Console.WriteLine("\n--- Skills ---");
        foreach (var s in skills)
        {
            string sec = s.SecondaryAttribute.HasValue ? s.SecondaryAttribute.ToString()! : "—";
            Console.WriteLine($"  [{s.Id}] {s.Name} (Primary: {s.PrimaryAttribute}, Secondary: {sec})");
        }
    }

    private void CreateSkill()
    {
        Console.Write("Skill name: ");
        var name = Console.ReadLine() ?? "";
        Console.Write("Description: ");
        var desc = Console.ReadLine() ?? "";
        var primary = PromptAttribute("Primary attribute");
        if (primary is null) return;
        Console.Write("Secondary attribute (blank for none): ");
        var secInput = Console.ReadLine()?.Trim();
        CoreAttribute? secondary = null;
        if (!string.IsNullOrEmpty(secInput) && Enum.TryParse<CoreAttribute>(secInput, true, out var sec))
            secondary = sec;

        var skill = new Skill { Name = name, Description = desc, PrimaryAttribute = primary.Value, SecondaryAttribute = secondary };
        _dbContext.AddEntity(skill);
        _dbContext.SaveChanges();
        Console.WriteLine($"Skill '{name}' created (ID {skill.Id}).");
    }

    private Skill? PromptSkill(string verb)
    {
        ListSkills();
        Console.Write($"\nSkill ID to {verb}: ");
        if (!int.TryParse(Console.ReadLine(), out var id)) { Console.WriteLine("Invalid."); return null; }
        var s = _dbContext.Skills.FirstOrDefault(x => x.Id == id);
        if (s is null) Console.WriteLine("Not found.");
        return s;
    }

    private void EditSkillDef()
    {
        var s = PromptSkill("edit");
        if (s is null) return;
        Console.Write($"Name [{s.Name}] (blank to keep): ");
        var n = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(n)) s.Name = n.Trim();
        Console.Write($"Description [{s.Description}] (blank to keep): ");
        var d = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(d)) s.Description = d.Trim();
        _dbContext.SaveChanges();
        Console.WriteLine("Skill updated.");
    }

    private void DeleteSkillDef()
    {
        var s = PromptSkill("delete");
        if (s is null) return;
        _dbContext.RemoveEntity(s);
        _dbContext.SaveChanges();
        Console.WriteLine("Skill deleted.");
    }

    private void AssignSkillToCharacter()
    {
        var skill = PromptSkill("assign");
        if (skill is null) return;
        var character = ResolveActiveOrPrompt("assign skill to");
        if (character is null) return;
        if (character.CharacterSkills.Any(cs => cs.SkillId == skill.Id))
        {
            Console.WriteLine($"{character.Name} already has {skill.Name}.");
            return;
        }
        Console.Write("Proficiency (integer): ");
        if (!int.TryParse(Console.ReadLine(), out var prof)) { Console.WriteLine("Invalid."); return; }
        var cs = new CharacterSkill { CharacterId = character.Id, SkillId = skill.Id, Proficiency = prof };
        _dbContext.AddEntity(cs);
        _dbContext.SaveChanges();
        Console.WriteLine($"Assigned {skill.Name} (Prof {prof}) to {character.Name}.");
    }

    public void AbilitiesSubmenu()
    {
        while (true)
        {
            Console.WriteLine("\n=== Abilities ===");
            Console.WriteLine("  1. List   2. Create   3. Edit   4. Delete   5. Assign to character   0. Back");
            Console.Write("Choice: ");
            switch (Console.ReadLine()?.Trim())
            {
                case "1": ListAbilities(); break;
                case "2": CreateAbility(); break;
                case "3": EditAbilityDef(); break;
                case "4": DeleteAbilityDef(); break;
                case "5": AssignAbilityToCharacter(); break;
                case "0": return;
                default: Console.WriteLine("Invalid."); break;
            }
        }
    }

    private void ListAbilities()
    {
        var abilities = _dbContext.Abilities.ToList();
        if (!abilities.Any()) { Console.WriteLine("\nNo abilities."); return; }
        Console.WriteLine("\n--- Abilities ---");
        foreach (var a in abilities)
            Console.WriteLine($"  [{a.Id}] {a.Name} ({a.Kind}, Power {a.Power}, Cost {a.StaminaCost} SP, Stat {a.PrimaryStat})");
    }

    private void CreateAbility()
    {
        Console.Write("Ability name: ");
        var name = Console.ReadLine() ?? "";
        Console.Write("Description: ");
        var desc = Console.ReadLine() ?? "";
        Console.Write("Power: ");
        int.TryParse(Console.ReadLine(), out var power);
        Console.Write("Stamina cost: ");
        int.TryParse(Console.ReadLine(), out var cost);
        Console.Write($"Kind ({string.Join("/", Enum.GetNames<AbilityKind>())}): ");
        if (!Enum.TryParse<AbilityKind>(Console.ReadLine(), true, out var kind)) { Console.WriteLine("Invalid kind."); return; }
        var stat = PromptAttribute("Primary stat");
        if (stat is null) return;
        var ab = new Ability { Name = name, Description = desc, Power = power, StaminaCost = cost, Kind = kind, PrimaryStat = stat.Value };
        _dbContext.AddEntity(ab);
        _dbContext.SaveChanges();
        Console.WriteLine($"Ability '{name}' created (ID {ab.Id}).");
    }

    private Ability? PromptAbility(string verb)
    {
        ListAbilities();
        Console.Write($"\nAbility ID to {verb}: ");
        if (!int.TryParse(Console.ReadLine(), out var id)) { Console.WriteLine("Invalid."); return null; }
        var a = _dbContext.Abilities.FirstOrDefault(x => x.Id == id);
        if (a is null) Console.WriteLine("Not found.");
        return a;
    }

    private void EditAbilityDef()
    {
        var a = PromptAbility("edit");
        if (a is null) return;
        Console.Write($"Name [{a.Name}] (blank to keep): ");
        var n = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(n)) a.Name = n.Trim();
        Console.Write($"Description [{a.Description}] (blank to keep): ");
        var d = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(d)) a.Description = d.Trim();
        Console.Write($"Power [{a.Power}] (blank to keep): ");
        if (int.TryParse(Console.ReadLine(), out var p)) a.Power = p;
        Console.Write($"Stamina cost [{a.StaminaCost}] (blank to keep): ");
        if (int.TryParse(Console.ReadLine(), out var c)) a.StaminaCost = c;
        _dbContext.SaveChanges();
        Console.WriteLine("Ability updated.");
    }

    private void DeleteAbilityDef()
    {
        var a = PromptAbility("delete");
        if (a is null) return;
        _dbContext.RemoveEntity(a);
        _dbContext.SaveChanges();
        Console.WriteLine("Ability deleted.");
    }

    private void AssignAbilityToCharacter()
    {
        var ab = PromptAbility("assign");
        if (ab is null) return;
        var character = ResolveActiveOrPrompt("assign ability to");
        if (character is null) return;
        if (character.Abilities.Any(x => x.Id == ab.Id))
        {
            Console.WriteLine($"{character.Name} already has {ab.Name}.");
            return;
        }
        character.Abilities.Add(ab);
        _dbContext.SaveChanges();
        Console.WriteLine($"Assigned {ab.Name} to {character.Name}.");
    }

    public void MagicSubmenu()
    {
        while (true)
        {
            Console.WriteLine("\n=== Magic ===");
            Console.WriteLine("  1. List   2. Create   3. Edit   4. Delete   5. Assign to character   0. Back");
            Console.Write("Choice: ");
            switch (Console.ReadLine()?.Trim())
            {
                case "1": ListMagic(); break;
                case "2": CreateMagic(); break;
                case "3": EditMagicDef(); break;
                case "4": DeleteMagicDef(); break;
                case "5": AssignMagicToCharacter(); break;
                case "0": return;
                default: Console.WriteLine("Invalid."); break;
            }
        }
    }

    private void ListMagic()
    {
        var magics = _dbContext.Magics.ToList();
        if (!magics.Any()) { Console.WriteLine("\nNo magic defined."); return; }
        Console.WriteLine("\n--- Magic ---");
        foreach (var m in magics)
            Console.WriteLine($"  [{m.Id}] {m.Name} ({m.Element}, {m.Kind}) Power {m.Power}, BitPool {m.BitPoolCost}, Bytes {m.BytePoolCost}, Stat {m.PrimaryStat}");
    }

    private void CreateMagic()
    {
        Console.Write("Magic name: ");
        var name = Console.ReadLine() ?? "";
        Console.Write("Description: ");
        var desc = Console.ReadLine() ?? "";
        Console.Write("Power: ");
        int.TryParse(Console.ReadLine(), out var power);
        Console.Write("BitPool cost: ");
        int.TryParse(Console.ReadLine(), out var bit);
        Console.Write("BytePool cost: ");
        int.TryParse(Console.ReadLine(), out var bytes);
        Console.Write($"Element ({string.Join("/", Enum.GetNames<Element>())}): ");
        if (!Enum.TryParse<Element>(Console.ReadLine(), true, out var element)) { Console.WriteLine("Invalid element."); return; }
        Console.Write($"Kind ({string.Join("/", Enum.GetNames<MagicKind>())}): ");
        if (!Enum.TryParse<MagicKind>(Console.ReadLine(), true, out var kind)) { Console.WriteLine("Invalid kind."); return; }
        var stat = PromptAttribute("Primary stat");
        if (stat is null) return;

        var m = new MagicEntity
        {
            Name = name, Description = desc, Power = power,
            BitPoolCost = bit, BytePoolCost = bytes,
            Element = element, Kind = kind, PrimaryStat = stat.Value
        };
        _dbContext.AddEntity(m);
        _dbContext.SaveChanges();
        Console.WriteLine($"Magic '{name}' created (ID {m.Id}).");
    }

    private MagicEntity? PromptMagic(string verb)
    {
        ListMagic();
        Console.Write($"\nMagic ID to {verb}: ");
        if (!int.TryParse(Console.ReadLine(), out var id)) { Console.WriteLine("Invalid."); return null; }
        var m = _dbContext.Magics.FirstOrDefault(x => x.Id == id);
        if (m is null) Console.WriteLine("Not found.");
        return m;
    }

    private void EditMagicDef()
    {
        var m = PromptMagic("edit");
        if (m is null) return;
        Console.Write($"Name [{m.Name}] (blank to keep): ");
        var n = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(n)) m.Name = n.Trim();
        Console.Write($"Description [{m.Description}] (blank to keep): ");
        var d = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(d)) m.Description = d.Trim();
        Console.Write($"Power [{m.Power}] (blank to keep): ");
        if (int.TryParse(Console.ReadLine(), out var p)) m.Power = p;
        Console.Write($"BitPool cost [{m.BitPoolCost}] (blank to keep): ");
        if (int.TryParse(Console.ReadLine(), out var bp)) m.BitPoolCost = bp;
        Console.Write($"BytePool cost [{m.BytePoolCost}] (blank to keep): ");
        if (int.TryParse(Console.ReadLine(), out var by)) m.BytePoolCost = by;
        _dbContext.SaveChanges();
        Console.WriteLine("Magic updated.");
    }

    private void DeleteMagicDef()
    {
        var m = PromptMagic("delete");
        if (m is null) return;
        _dbContext.RemoveEntity(m);
        _dbContext.SaveChanges();
        Console.WriteLine("Magic deleted.");
    }

    private void AssignMagicToCharacter()
    {
        var m = PromptMagic("assign");
        if (m is null) return;
        var character = ResolveActiveOrPrompt("assign magic to");
        if (character is null) return;
        if (character.Magics.Any(x => x.Id == m.Id))
        {
            Console.WriteLine($"{character.Name} already knows {m.Name}.");
            return;
        }
        character.Magics.Add(m);
        _dbContext.SaveChanges();
        Console.WriteLine($"Assigned {m.Name} to {character.Name}.");
    }

    private static CoreAttribute? PromptAttribute(string label)
    {
        Console.Write($"{label} ({string.Join("/", Enum.GetNames<CoreAttribute>())}): ");
        if (Enum.TryParse<CoreAttribute>(Console.ReadLine(), true, out var attr)) return attr;
        Console.WriteLine("Invalid attribute.");
        return null;
    }
}

// -- small local extension to get the un-proxied item TPH type name --
internal static class ItemTypeNameExtensions
{
    public static string TypeNameForItem(this Item item)
    {
        var t = item.GetType();
        // Walk up proxy chain until we land on something inside ConsoleRpgEntities.Models.Items.
        while (t != null && t.Namespace != "ConsoleRpgEntities.Models.Items")
            t = t.BaseType;
        return t?.Name ?? item.GetType().Name;
    }
}
