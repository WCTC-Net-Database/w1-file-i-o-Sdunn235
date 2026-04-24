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

        var stats = new Stats
        {
            CharacterId = character.Id,
            Physique = 5, Reflexes = 5, Constitution = 5,
            Intellect = 5, Intuition = 5, Linguistic = 5, Luck = 5
        };
        _dbContext.AddEntity(stats);

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
    // Equipment (W11 carryover — slot-centric view)
    // -------------------------------------------------------------------------

    public void DisplayEquipment()
    {
        var player = ResolveActivePlayer();
        if (player is null) return;

        Console.WriteLine($"\n=== {player.Name}'s Equipment ===\n");

        if (!player.EquipmentSlots.Any())
        {
            Console.WriteLine("  No equipment slots.");
            return;
        }

        foreach (var slot in player.EquipmentSlots)
        {
            string itemLabel = slot.EquippedItem?.Name ?? "(empty)";
            Console.WriteLine($"  {slot.Slot}: {itemLabel}");
        }

        Console.WriteLine($"\n  Total Attack:  {player.GetTotalAttack()}");
        Console.WriteLine($"  Total Defense: {player.GetTotalDefense()}");
    }

    // -------------------------------------------------------------------------
    // Items
    // -------------------------------------------------------------------------

    public void AddItem()
    {
        Console.Write("Item type (1=Weapon, 2=Armor, 3=Consumable): ");
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
