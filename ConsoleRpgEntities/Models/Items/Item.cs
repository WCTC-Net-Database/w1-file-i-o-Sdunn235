using System.ComponentModel.DataAnnotations.Schema;
using ConsoleRpgEntities.Models.Containers;
using ConsoleRpgEntities.Models.Enums;

namespace ConsoleRpgEntities.Models.Items;

public abstract class Item
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Value { get; set; }
    public int Weight { get; set; }
    public bool IsKeyItem { get; set; }

    // W13 — when IsKeyItem is true: null = generic lockpick (consumed on use),
    // non-null = specific key matching Chest.RequiredKeyId. Ignored when IsKeyItem is false.
    public string? KeyId { get; set; }

    // W12: every item lives in at most one container. Nullable so items can float
    // (e.g., orphaned during a move, or dropped in a room before W14 rooms-as-containers).
    public int? ContainerId { get; set; }
    public virtual Container? Container { get; set; }

    // W13: derived equip-slot eligibility. Consumables/KeyItems return null
    // (not equippable). Weapon/Armor override. NotMapped — derived from existing data.
    [NotMapped]
    public virtual SlotType? EligibleSlot => null;
}
