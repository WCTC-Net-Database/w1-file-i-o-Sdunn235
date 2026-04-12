using System.Text.Json.Serialization;
using ConsoleRpgEntities.Interfaces;

namespace ConsoleRpgEntities.Models;

/// <summary>
/// Abstract base class for all monsters loaded from monsters.json.
/// Uses JSON polymorphism so GameContext can deserialize concrete types (Goblin, Dragon, etc.)
/// without losing their specific behavior.
///
/// HOW TO ADD A NEW MONSTER TYPE (exam practice):
///   1. Create a class inheriting from MonsterBase
///   2. Add [JsonDerivedType(typeof(YourMonster), "yourtype")] to this class
///   3. Add an entry to monsters.json with "$type": "yourtype"
///   4. GameContext will load it automatically — no other changes needed (OCP)
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(Dragon), "dragon")]
public abstract class MonsterBase : IMonster
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public int Hp { get; set; }
    public int AttackPower { get; set; }

    /// <summary>
    /// Each monster defines its own special ability.
    /// Abstract enforces that every concrete monster must implement this — no empty stubs.
    /// </summary>
    public abstract void PerformSpecialAction();

    public override string ToString() =>
        $"[Lv {Level}] {Name} | HP: {Hp} | ATK: {AttackPower}";
}
