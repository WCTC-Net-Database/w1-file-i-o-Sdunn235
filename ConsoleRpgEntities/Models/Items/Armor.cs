using ConsoleRpgEntities.Models.Enums;

namespace ConsoleRpgEntities.Models.Items;

public class Armor : DurableItem
{
    public int DefenseRating { get; set; }
    public ArmorWeight WeightClass { get; set; }
    public BodySlot Slot { get; set; }
}
