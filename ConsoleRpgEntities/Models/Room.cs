namespace ConsoleRpgEntities.Models;

/// <summary>
/// Represents a location in the game world.
/// W9: First entity managed by EF Core — stored in SQL Server via GameContext (DbContext).
///
/// Navigation property Characters allows EF Core to load all characters in a room
/// with a single Include() call — no manual joins needed.
/// </summary>
public class Room
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>Navigation collection — all characters currently in this room (virtual for lazy loading).</summary>
    public virtual ICollection<Character> Characters { get; set; } = new List<Character>();
}
