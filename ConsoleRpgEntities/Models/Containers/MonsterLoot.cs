namespace ConsoleRpgEntities.Models.Containers;

// W13 — A Container subclass attached to a monster.
// Unreachable while the monster lives; lootable after defeat.
// Notably does NOT implement ILockable — you can't lock a corpse.
public class MonsterLoot : Container
{
    public bool IsLooted { get; set; }
}
