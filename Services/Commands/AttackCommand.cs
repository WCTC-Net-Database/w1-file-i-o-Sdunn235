using W6SolidDip.Interfaces;

namespace W6SolidDip.Services.Commands;

/// <summary>Encapsulates an Attack action for any IEntity (Command Pattern).</summary>
public class AttackCommand : ICommand
{
    private readonly IEntity _entity;

    public AttackCommand(IEntity entity) { _entity = entity; }

    public void Execute() { _entity.Attack(); }
}
