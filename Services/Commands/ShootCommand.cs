using W5SolidLsp.Interfaces;

namespace W5SolidLsp.Services.Commands;

/// <summary>Encapsulates a Shoot action for any IShootable entity (Command Pattern).</summary>
public class ShootCommand : ICommand
{
    private readonly IShootable _entity;

    public ShootCommand(IShootable entity) { _entity = entity; }

    public void Execute() { _entity.Shoot(); }
}
