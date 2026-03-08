using W6SolidDip.Interfaces;

namespace W6SolidDip.Services.Commands;

/// <summary>Encapsulates a Fly action for any IFlyable entity (Command Pattern).</summary>
public class FlyCommand : ICommand
{
    private readonly IFlyable _entity;

    public FlyCommand(IFlyable entity) { _entity = entity; }

    public void Execute() { _entity.Fly(); }
}
