using ConsoleRpgEntities.Models.Abilities;
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

    // Room — nullable so characters can exist without a room
    public int? RoomId { get; set; }
    public virtual Room? Room { get; set; }

    // Race — composition, not inheritance
    public int? RaceId { get; set; }
    public virtual Race? Race { get; set; }

    // One-to-one: core attributes and life resources
    public virtual Stats? Stats { get; set; }
    public virtual Resources? Resources { get; set; }

    // Equipment slots (one-to-many)
    public virtual ICollection<EquipmentSlot> EquipmentSlots { get; set; } = new List<EquipmentSlot>();

    // Many-to-many: abilities (physical techniques)
    public virtual ICollection<Ability> Abilities { get; set; } = new List<Ability>();

    // Many-to-many: magic (spells)
    public virtual ICollection<Magic.Magic> Magics { get; set; } = new List<Magic.Magic>();

    // Skills with proficiency tracking (explicit join entity)
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
}
