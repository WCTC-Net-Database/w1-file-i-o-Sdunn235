using W6SolidDip.Interfaces;

namespace W6SolidDip.Services.Commands;

/// <summary>Encapsulates a Swim action for any ISwimmable entity (Command Pattern).</summary>
public class SwimCommand : ICommand
{
    private readonly ISwimmable _entity;

    public SwimCommand(ISwimmable entity) { _entity = entity; }

    public void Execute() { _entity.Swim(); }
}
