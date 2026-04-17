using ConsoleRpgEntities.Models;
using ConsoleRpgEntities.Models.Abilities;
using ConsoleRpgEntities.Models.Items;
using ConsoleRpgEntities.Models.Magic;
using ConsoleRpgEntities.Models.Races;
using ConsoleRpgEntities.Models.Skills;

namespace ConsoleRpgEntities.Data;

public interface IContext
{
    IEnumerable<Room> Rooms { get; }
    IEnumerable<Door> Doors { get; }
    IEnumerable<Character> Characters { get; }
    IEnumerable<Race> Races { get; }
    IEnumerable<Stats> Stats { get; }
    IEnumerable<Resources> Resources { get; }
    IEnumerable<Ability> Abilities { get; }
    IEnumerable<Magic> Magics { get; }
    IEnumerable<Item> Items { get; }
    IEnumerable<EquipmentSlot> EquipmentSlots { get; }
    IEnumerable<Skill> Skills { get; }

    void AddEntity<T>(T entity) where T : class;
    void RemoveEntity<T>(T entity) where T : class;
    void SaveChanges();
}
