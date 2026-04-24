using ConsoleRpgEntities.Models.Abilities;
using ConsoleRpgEntities.Models.Containers;
using ConsoleRpgEntities.Models.Items;
using ConsoleRpgEntities.Models.Magic;
using ConsoleRpgEntities.Models.Races;
using ConsoleRpgEntities.Models.Skills;

namespace ConsoleRpgEntities.Models;

public abstract class Character
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }

    public int? RoomId { get; set; }
    public virtual Room? Room { get; set; }

    public int? RaceId { get; set; }
    public virtual Race? Race { get; set; }

    public virtual Stats? Stats { get; set; }
    public virtual Resources? Resources { get; set; }

    // W12: each character owns at most one Inventory and one Equipment container.
    // Nullable — a character can exist before getting a backpack or equipped gear.
    public virtual Inventory? Inventory { get; set; }
    public virtual Equipment? Equipment { get; set; }

    public virtual ICollection<EquipmentSlot> EquipmentSlots { get; set; } = new List<EquipmentSlot>();

    public virtual ICollection<Ability> Abilities { get; set; } = new List<Ability>();
    public virtual ICollection<Magic.Magic> Magics { get; set; } = new List<Magic.Magic>();
    public virtual ICollection<CharacterSkill> CharacterSkills { get; set; } = new List<CharacterSkill>();

    // --- Derivation helpers (bible §5: resources are derived from stats) ---

    public int DeriveMaxHp() => 50 + (Stats?.Constitution ?? 0) * 5;
    public int DeriveMaxSp() => 30 + (Stats?.Constitution ?? 0) * 3 + (Stats?.Reflexes ?? 0) * 2;
    public int DeriveMaxBp() => 20 + (Stats?.Intuition ?? 0) * 4;
    public int DeriveMaxBytePool() => 10 + (Stats?.Intellect ?? 0) * 3;

    // --- Combat helpers ---

    public int GetTotalAttack()
    {
        int baseAttack = Stats?.Physique ?? 0;
        int weaponBonus = EquipmentSlots
            .Where(s => s.EquippedItem is Weapon)
            .Sum(s => ((Weapon)s.EquippedItem!).AttackPower);
        return baseAttack + weaponBonus;
    }

    public int GetTotalDefense()
    {
        int baseDef = Stats?.Constitution ?? 0;
        int armorBonus = EquipmentSlots
            .Where(s => s.EquippedItem is Armor)
            .Sum(s => ((Armor)s.EquippedItem!).DefenseRating);
        return baseDef + armorBonus;
    }

    // W12: un-proxy a lazy-loaded entity so GetType().Name returns "Player" not "PlayerProxy".
    public string TypeName => GetType().BaseType?.Name ?? GetType().Name;
}
