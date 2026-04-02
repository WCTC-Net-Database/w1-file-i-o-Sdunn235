namespace ConsoleRpgEntities.Models;

/// <summary>
/// Represents a character entity managed by EF Core.
/// W9: Stored in SQL Server via GameContext (DbContext).
///
/// Note: This is ConsoleRpgEntities.Models.Character — a simple EF Core entity.
/// It is NOT the same as ConsoleRpg.Models.Characters.Character (the W6 abstract class).
/// Different namespaces, different purposes, no collision.
///
/// RoomId is a foreign key — EF Core enforces referential integrity automatically.
/// The Room navigation property enables eager loading with Include().
/// </summary>
public class Character
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }

    /// <summary>Foreign key to the Room this character belongs to.</summary>
    public int RoomId { get; set; }

    /// <summary>Navigation property — the room this character is in.</summary>
    public Room Room { get; set; } = null!;
}
