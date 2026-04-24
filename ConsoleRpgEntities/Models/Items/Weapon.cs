using ConsoleRpgEntities.Models.Enums;

namespace ConsoleRpgEntities.Models.Items;

public class Weapon : DurableItem
{
    public int AttackPower { get; set; }
    public WeaponType WeaponType { get; set; }
}
