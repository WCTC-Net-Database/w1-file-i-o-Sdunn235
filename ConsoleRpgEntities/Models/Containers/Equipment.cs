using ConsoleRpgEntities.Models.Enums;
using ConsoleRpgEntities.Models.Items;

namespace ConsoleRpgEntities.Models.Containers;

public class Equipment : Container
{
    public int? OwnerCharacterId { get; set; }
    public virtual Character? Owner { get; set; }

    public virtual ICollection<EquipmentSlot> Slots { get; set; } = new List<EquipmentSlot>();

    // W13 invariant: a Container subclass enforces a rule its base doesn't know about.
    // Adapted to W12's slot-based design: an item is equippable iff it has an
    // EligibleSlot AND the matching SlotType row exists and is empty.
    public bool CanEquip(Item item)
    {
        if (item.EligibleSlot is not SlotType slot) return false;
        var matching = Slots.FirstOrDefault(s => s.Slot == slot);
        return matching != null && matching.EquippedItemId == null;
    }
}
