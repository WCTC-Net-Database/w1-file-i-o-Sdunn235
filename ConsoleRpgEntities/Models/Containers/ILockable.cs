namespace ConsoleRpgEntities.Models.Containers;

// W13 — Interface Segregation: anything that can be locked, trapped, or picked.
// Implemented by Chest now. Will be reused on Door in W14 — same unlock/pick logic,
// different target. A MonsterLoot does not implement this (you can't lock a corpse).
public interface ILockable
{
    bool IsLocked { get; set; }
    bool IsTrapped { get; set; }
    bool IsPickable { get; set; }
    string? RequiredKeyId { get; set; }
    int TrapDamage { get; set; }
    bool TrapDisarmed { get; set; }
    int UnlockDC { get; set; }
}
