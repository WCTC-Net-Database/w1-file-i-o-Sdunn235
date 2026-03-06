using W6DependencyInversion.Interfaces;

namespace W6DependencyInversion.Services.Commands;

/// <summary>Encapsulates a Shoot action for any IShootable entity (Command Pattern).</summary>
public class ShootCommand : ICommand
{
    private readonly IShootable _entity;

    public ShootCommand(IShootable entity) { _entity = entity; }

    public void Execute() { _entity.Shoot(); }
}
