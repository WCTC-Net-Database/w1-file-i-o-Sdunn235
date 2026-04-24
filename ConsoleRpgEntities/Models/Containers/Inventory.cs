namespace ConsoleRpgEntities.Models.Containers;

public class Inventory : Container
{
    public int? OwnerCharacterId { get; set; }
    public virtual Character? Owner { get; set; }

    public int MaxWeight { get; set; } = 100;

    public int CurrentWeight => ItemsCollection.Sum(i => i.Weight);

    public bool CanFit(int additionalWeight) => CurrentWeight + additionalWeight <= MaxWeight;
}
