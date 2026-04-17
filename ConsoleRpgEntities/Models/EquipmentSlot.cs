using ConsoleRpgEntities.Models.Enums;
using ConsoleRpgEntities.Models.Items;

namespace ConsoleRpgEntities.Models;

public class EquipmentSlot
{
    public int Id { get; set; }
    public SlotType Slot { get; set; }

    public int CharacterId { get; set; }
    public virtual Character Character { get; set; } = null!;

    public int? EquippedItemId { get; set; }
    public virtual Item? EquippedItem { get; set; }
}
