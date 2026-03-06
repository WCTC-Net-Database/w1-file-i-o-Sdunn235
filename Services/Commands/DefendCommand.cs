using W6DependencyInversion.Interfaces;

namespace W6DependencyInversion.Services.Commands;

/// <summary>Encapsulates a Defend action for any IDefendable entity (Command Pattern).</summary>
public class DefendCommand : ICommand
{
    private readonly IDefendable _entity;

    public DefendCommand(IDefendable entity) { _entity = entity; }

    public void Execute() { _entity.Defend(); }
}
