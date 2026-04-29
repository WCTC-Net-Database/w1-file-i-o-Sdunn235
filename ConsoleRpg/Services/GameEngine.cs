using ConsoleRpg.UI;
using ConsoleRpgEntities.Data;
using ConsoleRpgEntities.Models;
using ConsoleRpgEntities.Models.Containers;
using ConsoleRpgEntities.Models.Enums;
using ConsoleRpgEntities.Models.Items;
using ConsoleRpgEntities.Models.Races;

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
        // 1. Backfill missing Stats rows.
        var orphans = _dbContext.Characters.Where(c => c.Stats == null).ToList();
        foreach (var c in orphans)
            _dbContext.AddEntity(new Stats { CharacterId = c.Id });
        if (orphans.Count > 0)
            _dbContext.SaveChanges();

        // 2. Sweep EquipmentSlots for slot/item mismatches.
        int slotsCleared = 0;
        var slots = _dbContext.EquipmentSlots
            .Where(s => s.EquippedItemId != null)
            .ToList();

        foreach (var slot in slots)
        {
            var item = slot.EquippedItem;
            if (item is null) continue;

            // Shield items can occupy MainHand or OffHand (handled in Shield class).
            // Everything else: EligibleSlot must equal the slot's SlotType, else clear.
            var eligible = item.EligibleSlot;
            bool valid = eligible.HasValue && eligible.Value == slot.Slot;

            if (item is Shield && (slot.Slot == SlotType.MainHand || slot.Slot == SlotType.OffHand))
                valid = true;

            if (!valid)
            {
                slot.EquippedItemId = null;
                slotsCleared++;
            }
        }

        if (slotsCleared > 0) _dbContext.SaveChanges();

        if (orphans.Count > 0 || slotsCleared > 0)
            Console.WriteLine($"[Startup] Integrity sweep: backfilled {orphans.Count} Stats row(s), cleared {slotsCleared} invalid equipment slot(s).");
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
            Bp = character.DeriveMaxBp(), MaxBp = character.DeriveMaxBp(),
            BytePool = character.DeriveMaxBytePool(), MaxBytePool = character.DeriveMaxBytePool()
        };
        _dbContext.AddEntity(resources);
        _dbContext.SaveChanges();

        Console.WriteLine($"\n{character.TypeName} '{name}' created.");
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
        if (c is not Player player)
        {
            Console.WriteLine($"\n{c.Name} is a {c.TypeName} — equipment management is Player-only.");
            return;
        }

        while (true)
        {
            Console.WriteLine($"\n--- Equipment: {player.Name} ---");
            if (player.EquipmentSlots.Any())
                foreach (var slot in player.EquipmentSlots)
                    Console.WriteLine($"  {slot.Slot,-9} {slot.EquippedItem?.Name ?? "(empty)"}");
            else
                Console.WriteLine("  (no slots)");

            Console.WriteLine("  1. Equip from inventory   2. Unequip   0. Back");
            Console.Write("Choice: ");
            var ch = Console.ReadLine()?.Trim();
            switch (ch)
            {
                case "1": InventoryEquip(player); break;
                case "2": InventoryUnequip(player); break;
                case "0": return;
                default: Console.WriteLine("Invalid."); break;
            }
        }
    }

    private void EditInventory(Character c)
    {
        if (c is not Player player)
        {
            Console.WriteLine($"\n{c.Name} is a {c.TypeName} — inventory is Player-only.");
            return;
        }
        if (player.Inventory is null)
        {
            Console.WriteLine($"\n{player.Name} has no inventory container.");
            return;
        }

        while (true)
        {
            var items = Items(player).ToList();
            Console.WriteLine($"\n--- Inventory: {player.Name} ({items.Count} items) ---");
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
                    if (picked != null) { picked.ContainerId = player.Inventory.Id; _dbContext.SaveChanges(); Console.WriteLine($"Added {picked.Name}."); }
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
            Console.WriteLine($"  HP: {r.Hp}/{r.MaxHp} (derived max: {character.DeriveMaxHp()})");
            Console.WriteLine($"  SP: {r.Sp}/{r.MaxSp} (derived max: {character.DeriveMaxSp()})");
            Console.WriteLine($"  BP: {r.Bp}/{r.MaxBp} (derived max: {character.DeriveMaxBp()})");
            Console.WriteLine($"  BytePool: {r.BytePool}/{r.MaxBytePool} (derived max: {character.DeriveMaxBytePool()})");
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
                Console.WriteLine($"  {m.Name} ({m.Element}) Power: {m.Power}, BP: {m.BpCost}, Bytes: {m.BytePoolCost}");
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

    private Player? ResolveActivePlayer()
    {
        var character = ResolveActiveOrPrompt("use for inventory");
        if (character is Player p) return p;
        if (character is not null)
            Console.WriteLine($"\n{character.Name} is a {character.TypeName}, not a Player — inventory actions are Player-only.");
        return null;
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

            if (r.Doors.Any())
            {
                foreach (var d in r.Doors)
                    Console.WriteLine($"      {d.Direction} → {d.DestinationRoom.Name}{(d.IsLocked ? " [LOCKED]" : "")}");
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

        Console.Write("Source room ID: ");
        if (!int.TryParse(Console.ReadLine(), out var sourceId)) { Console.WriteLine("Invalid."); return; }

        Console.Write("Destination room ID: ");
        if (!int.TryParse(Console.ReadLine(), out var destId)) { Console.WriteLine("Invalid."); return; }

        Console.Write("Direction (North/South/East/West/Up/Down): ");
        var dirInput = Console.ReadLine() ?? string.Empty;
        if (!Enum.TryParse<Direction>(dirInput, true, out var direction))
        {
            Console.WriteLine("Invalid direction.");
            return;
        }

        Console.Write("Door name (e.g., 'Oak Door'): ");
        var name = Console.ReadLine() ?? string.Empty;

        Console.Write("Locked? (y/n): ");
        bool locked = (Console.ReadLine()?.Trim().ToLower() ?? "") == "y";

        var door = new Door
        {
            Name = name,
            Description = $"A passage leading {direction}",
            Direction = direction,
            IsLocked = locked,
            SourceRoomId = sourceId,
            DestinationRoomId = destId
        };

        _dbContext.AddEntity(door);
        _dbContext.SaveChanges();

        Console.WriteLine($"\nDoor '{name}' connects Room {sourceId} → Room {destId} ({direction}).");
    }

    public void DisplayCurrentRoom()
    {
        var player = ResolveActivePlayer();
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

        if (room.Doors.Any())
        {
            Console.WriteLine("\n  Exits:");
            foreach (var d in room.Doors)
                Console.WriteLine($"    {d.Direction} — {d.Name} → {d.DestinationRoom.Name}{(d.IsLocked ? " [LOCKED]" : "")}");
        }
        else
        {
            Console.WriteLine("\n  No exits.");
        }
    }

    public void MovePlayer()
    {
        var player = ResolveActivePlayer();
        if (player is null) return;
        if (player.Room is null) { Console.WriteLine($"\n{player.Name} is not in any room."); return; }

        DisplayCurrentRoom();

        Console.Write("\nDirection to move: ");
        var dirInput = Console.ReadLine() ?? string.Empty;
        if (!Enum.TryParse<Direction>(dirInput, true, out var direction))
        {
            Console.WriteLine("Invalid direction.");
            return;
        }

        var door = player.Room.Doors.FirstOrDefault(d => d.Direction == direction);
        if (door is null)
        {
            Console.WriteLine($"\nNo exit to the {direction}.");
            return;
        }

        if (door.IsLocked)
        {
            Console.WriteLine($"\nThe {door.Name} is locked!");
            return;
        }

        player.RoomId = door.DestinationRoomId;
        _dbContext.SaveChanges();

        Console.WriteLine($"\n{player.Name} moves {direction} through the {door.Name}.");
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
                Console.Write("Effect: ");
                var effect = Console.ReadLine() ?? string.Empty;
                Console.Write("Potency: ");
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
    // W12 — Inventory Management (Player only)
    // -------------------------------------------------------------------------

    public void InventoryMenu()
    {
        var player = ResolveActivePlayer();
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

    private static IEnumerable<Item> Items(Player p) =>
        p.Inventory?.ItemsCollection ?? Enumerable.Empty<Item>();

    private void InventoryList(Player player)
    {
        var items = Items(player).ToList();
        var max = player.Inventory!.MaxWeight;
        var cur = items.Sum(i => i.Weight);

        Console.WriteLine($"\n--- Inventory ({items.Count} items, {cur} / {max} lbs) ---");
        if (!items.Any()) { Console.WriteLine("  (empty)"); return; }

        foreach (var i in items)
            Console.WriteLine($"  [{i.Id}] {i.Name} — {i.TypeNameForItem()}, {i.Weight} lbs, {i.Value}g");
    }

    private void InventorySearch(Player player)
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

    private void InventoryGroupByType(Player player)
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

    private void InventorySort(Player player)
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

    private void InventoryEquip(Player player)
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

    private void InventoryUnequip(Player player)
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

    private void InventoryUseConsumable(Player player)
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
    private void InventoryStrongestWeapon(Player player)
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
    private void InventoryTotalValueBreakdown(Player player)
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
    // W13 — Chest & Monster Loot Interaction (Player only)
    // -------------------------------------------------------------------------

    public void ChestMenu()
    {
        var player = ResolveActivePlayer();
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

    private List<Chest> ChestsInPlayerRoom(Player player) =>
        _dbContext.Containers
            .OfType<Chest>()
            .Where(c => c.RoomId == player.RoomId)
            .ToList();

    private void ChestList(Player player)
    {
        if (player.RoomId is null) { Console.WriteLine("\nPlayer is not in a room."); return; }

        var chests = ChestsInPlayerRoom(player);
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

    private Chest? PromptForChest(Player player)
    {
        var chests = ChestsInPlayerRoom(player);
        if (!chests.Any()) { Console.WriteLine("\nNo chests in this room."); return null; }

        Console.Write("Chest ID: ");
        if (!int.TryParse(Console.ReadLine(), out var id)) { Console.WriteLine("Invalid."); return null; }
        var chest = chests.FirstOrDefault(c => c.Id == id);
        if (chest is null) Console.WriteLine("Not in this room.");
        return chest;
    }

    private void ChestOpen(Player player)
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

    private void ChestTryUnlock(Player player)
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

    private void ChestDisarmTrap(Player player)
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

    private void ChestLoot(Player player)
    {
        var chest = PromptForChest(player);
        if (chest is null) return;
        if (chest.IsLocked) { Console.WriteLine("\nLocked. Unlock first."); return; }

        int moved = player.LootChest(chest);
        _dbContext.SaveChanges();
        Console.WriteLine($"\nLooted {moved} item(s) from {chest.Name}.");
    }

    private void ChestLootMonster(Player player)
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

        int moved = player.LootMonster(monster);
        _dbContext.SaveChanges();
        Console.WriteLine($"\nLooted {moved} item(s) from {monster.Name}.");
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
