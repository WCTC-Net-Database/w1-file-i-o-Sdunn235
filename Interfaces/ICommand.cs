namespace W5SolidLsp.Interfaces;

/// <summary>
/// Interface for the Command Pattern (Stretch Goal).
/// Encapsulates a single action so it can be queued, logged, or undone.
///
/// Usage in GameEngine:
///   var commands = new List&lt;ICommand&gt;();
///   commands.Add(new AttackCommand(goblin));
///   commands.Add(new FlyCommand(ghost));
///   foreach (var cmd in commands) cmd.Execute();
/// </summary>
public interface ICommand
{
    /// <summary>Executes the encapsulated action.</summary>
    void Execute();
}
