namespace ConsoleRpgEntities.Models.Containers;

public class Equipment : Container
{
    public int? OwnerCharacterId { get; set; }
    public virtual Character? Owner { get; set; }

    public virtual ICollection<EquipmentSlot> Slots { get; set; } = new List<EquipmentSlot>();
}
