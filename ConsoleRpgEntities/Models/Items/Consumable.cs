namespace ConsoleRpgEntities.Models.Items;

public class Consumable : Item
{
    public string Effect { get; set; } = string.Empty;
    public int Potency { get; set; }
}
