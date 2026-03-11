using System.Text.Json;
using ConsoleRpgEntities.Models;

namespace ConsoleRpgEntities.Data;

/// <summary>
/// Primary data context — loads all game entities from JSON files into memory.
/// Implements IContext so ConsoleRpg always depends on the abstraction, not this class.
///
/// OCP: Adding a new entity type (e.g., Rooms) only requires adding a new list,
/// a new JSON file, and updating Read/Write/SaveChanges — no changes to ConsoleRpg.
///
/// Week 9: This class is replaced by DbContext backed by a real database.
/// ConsoleRpg needs zero changes because it only ever saw IContext.
/// </summary>
public class GameContext : IContext
{
    private readonly string _playersPath;
    private readonly string _monstersPath;
    private readonly string _itemsPath;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public List<Player> Players { get; private set; } = new();
    public List<MonsterBase> Monsters { get; private set; } = new();
    public List<Item> Items { get; private set; } = new();

    public GameContext(string playersPath, string monstersPath, string itemsPath)
    {
        _playersPath = playersPath;
        _monstersPath = monstersPath;
        _itemsPath = itemsPath;
    }

    /// <summary>Loads all entities from their JSON files into the in-memory lists.</summary>
    public void Read()
    {
        Players = LoadJson<List<Player>>(_playersPath) ?? new List<Player>();
        Monsters = LoadJson<List<MonsterBase>>(_monstersPath) ?? new List<MonsterBase>();
        Items = LoadJson<List<Item>>(_itemsPath) ?? new List<Item>();
    }

    /// <summary>Adds an entity to the appropriate in-memory list based on its type.</summary>
    public void Write(object entity)
    {
        switch (entity)
        {
            case Player player:
                Players.Add(player);
                break;
            case MonsterBase monster:
                Monsters.Add(monster);
                break;
            case Item item:
                Items.Add(item);
                break;
        }
    }

    /// <summary>
    /// Persists player state to players.json.
    /// Monsters are intentionally NOT saved here — monsters.json is the permanent source
    /// of truth that ResetBattle() reads back from. Writing damaged monster state would
    /// prevent the battle from ever resetting correctly.
    /// </summary>
    public void SaveChanges()
    {
        File.WriteAllText(_playersPath, JsonSerializer.Serialize(Players, _jsonOptions));
    }

    private static T? LoadJson<T>(string path)
    {
        if (!File.Exists(path)) return default;
        return JsonSerializer.Deserialize<T>(File.ReadAllText(path), _jsonOptions);
    }
}
