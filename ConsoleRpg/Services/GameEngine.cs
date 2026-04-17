using ConsoleRpg.UI;
using ConsoleRpgEntities.Data;
using ConsoleRpgEntities.Models;
using ConsoleRpgEntities.Models.Enums;
using ConsoleRpgEntities.Models.Items;
using ConsoleRpgEntities.Models.Races;

namespace ConsoleRpg.Services;

public class GameEngine
{
    private readonly IContext _dbContext;
    private readonly IGameUi _gameUi;

    public GameEngine(IContext dbContext, IGameUi gameUi)
    {
        _dbContext = dbContext;
        _gameUi = gameUi;
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
            Console.WriteLine($"  [{c.Id}] {c.Name} (Lv {c.Level}) — {raceLabel} — Room: {roomLabel}");
        }
    }

    public void FindCharacter()
    {
        Console.Write("Enter character name to search: ");
        var name = Console.ReadLine() ?? string.Empty;

        var character = _dbContext.Characters
            .FirstOrDefault(c => c.Name.Contains(name));

        if (character != null)
            Console.WriteLine($"\nFound: [{character.Id}] {character.Name} (Level {character.Level})");
        else
            Console.WriteLine("\nCharacter not found.");
    }

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

        Console.Write("Type (1=Player, 2=NPC, 3=Animal): ");
        var typeChoice = Console.ReadLine()?.Trim();

        // Show available races
        var races = _dbContext.Races.ToList();
        if (races.Any())
        {
            Console.WriteLine("\nAvailable Races:");
            foreach (var r in races)
                Console.WriteLine($"  [{r.Id}] {r.Name} ({r.GetType().Name})");
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

        // Show available rooms
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
                // Validate PlayableRace for players
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

        // Create default stats and resources
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

        Console.WriteLine($"\n{character.GetType().Name} '{name}' created.");
    }

    public void LevelUpCharacter()
    {
        Console.Write("Enter character name: ");
        var name = Console.ReadLine() ?? string.Empty;

        var character = _dbContext.Characters.FirstOrDefault(c => c.Name == name);
        if (character != null)
        {
            character.Level++;
            _dbContext.SaveChanges();
            Console.WriteLine($"\n{character.Name} is now level {character.Level}!");
        }
        else
        {
            Console.WriteLine("\nCharacter not found.");
        }
    }

    public void DisplayCharacterDetail()
    {
        Console.Write("Enter character name: ");
        var name = Console.ReadLine() ?? string.Empty;

        var character = _dbContext.Characters.FirstOrDefault(c => c.Name.Contains(name));
        if (character == null)
        {
            Console.WriteLine("\nCharacter not found.");
            return;
        }

        Console.WriteLine($"\n=== {character.Name} ({character.GetType().Name}) ===");
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
        var player = _dbContext.Characters.OfType<Player>().FirstOrDefault();
        if (player == null) { Console.WriteLine("\nNo player found."); return; }
        if (player.Room == null) { Console.WriteLine($"\n{player.Name} is not in any room."); return; }

        var room = player.Room;
        Console.WriteLine($"\n=== {room.Name} ===");
        Console.WriteLine($"  {room.Description}");

        // Characters in the room
        var others = room.Characters.Where(c => c.Id != player.Id).ToList();
        if (others.Any())
        {
            Console.WriteLine("\n  Characters here:");
            foreach (var c in others)
                Console.WriteLine($"    {c.Name} ({c.GetType().Name})");
        }

        // Exits
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
        var player = _dbContext.Characters.OfType<Player>().FirstOrDefault();
        if (player == null) { Console.WriteLine("\nNo player found."); return; }
        if (player.Room == null) { Console.WriteLine($"\n{player.Name} is not in any room."); return; }

        DisplayCurrentRoom();

        Console.Write("\nDirection to move: ");
        var dirInput = Console.ReadLine() ?? string.Empty;
        if (!Enum.TryParse<Direction>(dirInput, true, out var direction))
        {
            Console.WriteLine("Invalid direction.");
            return;
        }

        var door = player.Room.Doors.FirstOrDefault(d => d.Direction == direction);
        if (door == null)
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
    // Equipment
    // -------------------------------------------------------------------------

    public void DisplayEquipment()
    {
        var player = _dbContext.Characters.OfType<Player>().FirstOrDefault();
        if (player == null) { Console.WriteLine("\nNo player found."); return; }

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

    public void EquipItem()
    {
        var player = _dbContext.Characters.OfType<Player>().FirstOrDefault();
        if (player == null) { Console.WriteLine("\nNo player found."); return; }

        // Show available items
        var items = _dbContext.Items.ToList();
        if (!items.Any())
        {
            Console.WriteLine("\nNo items exist.");
            return;
        }

        Console.WriteLine("\n=== Available Items ===\n");
        foreach (var item in items)
        {
            string typeLabel = item switch
            {
                Weapon w => $"Weapon (ATK: {w.AttackPower}, {w.WeaponType})",
                Armor a => $"Armor (DEF: {a.DefenseRating}, {a.WeightClass}, {a.Slot})",
                Consumable c => $"Consumable ({c.Effect}, Potency: {c.Potency})",
                _ => item.GetType().Name
            };
            Console.WriteLine($"  [{item.Id}] {item.Name} — {typeLabel}");
        }

        Console.Write("\nItem ID to equip: ");
        if (!int.TryParse(Console.ReadLine(), out var itemId)) { Console.WriteLine("Invalid."); return; }

        Console.Write("Slot (MainHand/OffHand/Head/Chest/Legs/Feet/Hands): ");
        var slotInput = Console.ReadLine() ?? string.Empty;
        if (!Enum.TryParse<SlotType>(slotInput, true, out var slotType))
        {
            Console.WriteLine("Invalid slot.");
            return;
        }

        // Find or create the slot
        var slot = player.EquipmentSlots.FirstOrDefault(s => s.Slot == slotType);
        if (slot == null)
        {
            slot = new EquipmentSlot { CharacterId = player.Id, Slot = slotType };
            _dbContext.AddEntity(slot);
        }

        slot.EquippedItemId = itemId;
        _dbContext.SaveChanges();

        var equippedItem = _dbContext.Items.FirstOrDefault(i => i.Id == itemId);
        Console.WriteLine($"\n{equippedItem?.Name ?? "Item"} equipped to {slotType}.");
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
        Console.WriteLine($"\n{item.GetType().Name} '{name}' created.");
    }
}
