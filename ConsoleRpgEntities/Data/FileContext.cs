using System.Text.Json;
using ConsoleRpgEntities.Models;

namespace ConsoleRpgEntities.Data;

/// <summary>
/// File-based data context — loads all game entities from JSON files into memory.
/// Implements IContext so ConsoleRpg always depends on the abstraction, not this class.
///
/// Week 7: Originally named GameContext — the sole IContext implementation.
/// Week 9: Renamed to FileContext to make room for the EF Core GameContext (DbContext).
///         Both implement IContext — business logic never knows which back-end is in use.
///
/// OCP: Adding a new entity type only requires a new list, a new JSON file,
/// and updating Read/AddEntity/RemoveEntity/SaveChanges.
/// </summary>
public class FileContext : IContext
{
    private readonly string _playersPath;
    private readonly string _monstersPath;
    private readonly string _itemsPath;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    // Internal storage is List<T>, exposed as IEnumerable<T> through IContext
    private List<Player> _players = new();
    private List<MonsterBase> _monsters = new();
    private List<Item> _items = new();

    // IContext — existing entity collections
    public IEnumerable<Player> Players => _players;
    public IEnumerable<MonsterBase> Monsters => _monsters;
    public IEnumerable<Item> Items => _items;

    // IContext — W9 entity collections (not file-backed yet — returns empty)
    public IEnumerable<Room> Rooms => Enumerable.Empty<Room>();
    public IEnumerable<Character> Characters => Enumerable.Empty<Character>();

    public FileContext(string playersPath, string monstersPath, string itemsPath)
    {
        _playersPath = playersPath;
        _monstersPath = monstersPath;
        _itemsPath = itemsPath;
    }

    /// <summary>
    /// Loads all entities from their JSON files into the in-memory lists.
    /// This is a FileContext-specific operation — not part of IContext.
    /// Called once at startup and again by ResetBattle to restore original state.
    /// </summary>
    public void Read()
    {
        _players = LoadJson<List<Player>>(_playersPath) ?? new List<Player>();
        _monsters = LoadJson<List<MonsterBase>>(_monstersPath) ?? new List<MonsterBase>();
        _items = LoadJson<List<Item>>(_itemsPath) ?? new List<Item>();
    }

    /// <summary>
    /// Adds an entity to the appropriate in-memory list based on its runtime type.
    /// Type-safe generic version of the old Write(object) method.
    /// </summary>
    public void AddEntity<T>(T entity) where T : class
    {
        switch (entity)
        {
            case Player player:
                _players.Add(player);
                break;
            case MonsterBase monster:
                _monsters.Add(monster);
                break;
            case Item item:
                _items.Add(item);
                break;
        }
    }

    /// <summary>
    /// Removes an entity from the appropriate in-memory list based on its runtime type.
    /// </summary>
    public void RemoveEntity<T>(T entity) where T : class
    {
        switch (entity)
        {
            case Player player:
                _players.Remove(player);
                break;
            case MonsterBase monster:
                _monsters.Remove(monster);
                break;
            case Item item:
                _items.Remove(item);
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
        File.WriteAllText(_playersPath, JsonSerializer.Serialize(_players, _jsonOptions));
    }

    private static T? LoadJson<T>(string path)
    {
        if (!File.Exists(path)) return default;
        return JsonSerializer.Deserialize<T>(File.ReadAllText(path), _jsonOptions);
    }
}
