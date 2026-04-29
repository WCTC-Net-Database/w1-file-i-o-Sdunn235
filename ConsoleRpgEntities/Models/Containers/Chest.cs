namespace ConsoleRpgEntities.Models.Containers;

// W13 — A Container subclass that adds lock/trap/pick state.
// Two interfaces: IItemContainer (via Container) AND ILockable (state-based gating).
// Lives in a Room (nullable so chests can be created before placement).
public class Chest : Container, ILockable
{
    public string Description { get; set; } = string.Empty;

    // ILockable
    public bool IsLocked { get; set; }
    public bool IsTrapped { get; set; }
    public bool IsPickable { get; set; } = true;
    public string? RequiredKeyId { get; set; }
    public int TrapDamage { get; set; }
    public bool TrapDisarmed { get; set; }

    // LucentForge integration — DC for the lockpick skill check
    // (Reflexes + Lockpicking proficiency vs. UnlockDC). 0 means no skill check.
    public int UnlockDC { get; set; }

    public int? RoomId { get; set; }
    public virtual Room? Room { get; set; }
}
