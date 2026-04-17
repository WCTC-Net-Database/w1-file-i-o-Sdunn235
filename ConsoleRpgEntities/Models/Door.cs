using ConsoleRpgEntities.Models.Enums;

namespace ConsoleRpgEntities.Models;

public class Door
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Direction Direction { get; set; }
    public bool IsLocked { get; set; }

    public int SourceRoomId { get; set; }
    public virtual Room SourceRoom { get; set; } = null!;

    public int DestinationRoomId { get; set; }
    public virtual Room DestinationRoom { get; set; } = null!;
}
