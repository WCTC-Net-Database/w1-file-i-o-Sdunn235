namespace ConsoleRpgEntities.Models.Items;

public abstract class Item
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Value { get; set; }
    public int Weight { get; set; }
    public bool IsKeyItem { get; set; }
}
