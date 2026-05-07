namespace ConsoleRpgEntities.Models.Containers;

/// <summary>
/// A navigable space in the world. Promoted to a 5th <see cref="Container"/>
/// TPH subclass in W14 Phase C — items on the floor of a room are now stored
/// in the same Items table as items in backpacks, chests, equipment slots, or
/// monster loot, all addressed via <c>Item.ContainerId</c>.
///
/// <para>
/// What Room inherits from <see cref="Container"/>:
/// <list type="bullet">
///   <item><description><c>Id</c> — the canonical room id; FKs from Character.RoomId, Door.RoomAId/RoomBId, Chest.RoomId all point at this.</description></item>
///   <item><description><c>Name</c> — the room's display name (e.g., "Entrance Hall").</description></item>
///   <item><description><c>ItemsCollection</c> — items on the floor of this room.</description></item>
///   <item><description><c>AddItem</c> / <c>RemoveItem</c> — drop/pickup semantics (ContainerId = room.Id, or null on pickup).</description></item>
/// </list>
/// </para>
///
/// <para>
/// What Room owns directly: <see cref="Description"/> (room-specific flavor text;
/// EF disambiguates this from <c>Chest.Description</c> via TPH-shadow column
/// <c>Room_Description</c>), <see cref="Characters"/> (who is here), and
/// <see cref="Doors"/> (passages to neighbors — bidirectional after Phase C.2).
/// </para>
///
/// <para>
/// Pre-Phase-C, Room was a standalone entity in its own <c>Rooms</c> table.
/// The Phase C.1 migration moved each Room row into <c>Containers</c> with
/// <c>ContainerType='Room'</c>, captured an old→new id mapping, repointed
/// the Character.RoomId / Door.SourceRoomId / Door.DestinationRoomId / Chest.RoomId
/// FKs through that mapping, and dropped the standalone <c>Rooms</c> table.
/// </para>
/// </summary>
public class Room : Container
{
    /// <summary>Flavor text describing the room. Stored as TPH-shadow column
    /// <c>Room_Description</c> to avoid colliding with <c>Chest.Description</c>.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Characters currently located in this room. Maintained via
    /// <c>Character.RoomId</c> back-reference.</summary>
    public virtual ICollection<Character> Characters { get; set; } = new List<Character>();

    /// <summary>Doors leaving this room. After Phase C.2, doors are
    /// bidirectional — a Door between Room A and Room B appears in both
    /// rooms' <see cref="Doors"/> collections via the RoomA/RoomB join.</summary>
    public virtual ICollection<Door> Doors { get; set; } = new List<Door>();
}
