using W6SolidDip.Interfaces;
using W6SolidDip.Models.Characters.Npcs.Monsters;

namespace W6SolidDip.Models.Characters.Npcs.Monsters;

/// <summary>
/// A ghost — a hostile, undead spirit that can fly.
/// Inherits IEntity from CharacterBase (via Monster ? Npc ? CharacterBase).
/// Also implements IFlyable — ghosts can fly.
///
/// LSP: Ghost safely substitutes for IEntity anywhere.
/// ISP: Ghost only implements interfaces it genuinely supports.
/// </summary>
public class Ghost : Monster, IFlyable
{
    public Ghost() : base("Ghost", "Undead", 3, 20, "none") { }

    public Ghost(string name, int level, int hp)
        : base(name, "Undead", level, hp, "none") { }

    /// <summary>Ghost attacks by chilling the target with an icy touch.</summary>
    public override void Attack()
    {
        Console.WriteLine($"{Name} reaches out with an icy touch!");
    }

    /// <summary>Ghost floats silently through the air.</summary>
    public void Fly()
    {
        Console.WriteLine($"{Name} drifts silently through the air!");
    }

    /// <summary>Ghost phases through walls, becoming temporarily untargetable.</summary>
    public override void PerformSpecialAction()
    {
        Console.WriteLine($"{Name} phases through solid matter, becoming untargetable!");
    }
}
