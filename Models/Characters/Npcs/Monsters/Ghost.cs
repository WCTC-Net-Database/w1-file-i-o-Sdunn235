using W5SolidLsp.Interfaces;
using W5SolidLsp.Models.Characters.Npcs.Monsters;

namespace W5SolidLsp.Models.Characters.Npcs.Monsters;

/// <summary>
/// A ghost — a hostile, undead spirit that can fly.
/// Implements IEntity (can attack) and IFlyable (can fly).
///
/// LSP demo: Ghost safely substitutes for IEntity anywhere.
/// ISP demo: Ghost only implements interfaces it genuinely supports.
/// </summary>
public class Ghost : Monster, IEntity, IFlyable
{
    public Ghost() : base("Ghost", "Undead", 3, 20, "none") { }

    public Ghost(string name, int level, int hp)
        : base(name, "Undead", level, hp, "none") { }

    /// <summary>Ghost attacks by chilling the target with an icy touch.</summary>
    public void Attack()
    {
        Console.WriteLine($"{Name} reaches out with an icy touch!");
    }

    /// <summary>Ghost floats silently through the air.</summary>
    public void Fly()
    {
        Console.WriteLine($"{Name} drifts silently through the air!");
    }
}
