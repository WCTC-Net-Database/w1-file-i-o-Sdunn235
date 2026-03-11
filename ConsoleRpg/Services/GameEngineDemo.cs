using ConsoleRpg.Interfaces;
using ConsoleRpg.Models.Characters.Npcs.Monsters;
using ConsoleRpg.Models.Classes;

namespace ConsoleRpg.Services;

/// <summary>
/// Self-contained demonstration of W6 GameEngine concepts.
/// Extracted from Program.cs to honor SRP — the demo is a distinct, reusable concern.
///
/// Demonstrates:
///   - DIP: GameEngine depends on IEntity abstraction, not concrete types
///   - Abstract Classes: PerformSpecialAction called polymorphically on every entity
///   - LSP/ISP: Optional interfaces (IFlyable, ISwimmable, etc.) checked safely with 'is'
///
/// Accessible via menu option — not auto-run on startup.
/// </summary>
public class GameEngineDemo
{
    private readonly IFileHandler _fileHandler;

    /// <summary>
    /// DIP: Takes IFileHandler abstraction — demo never knows if it's CSV or JSON.
    /// </summary>
    public GameEngineDemo(IFileHandler fileHandler)
    {
        _fileHandler = fileHandler;
    }

    /// <summary>Runs the full W6 entity-turn demo.</summary>
    public void Run()
    {
        Console.WriteLine("=== W6: GameEngine Demo (DIP + Abstract Classes) ===\n");

        // DIP: GameEngine only sees IEntity — it never knows these are Ghost, Goblin, etc.
        var entities = new List<IEntity>
        {
            new Ghost("Shade", 3, 20),
            new Goblin("Gruk", 1, 15, "crude dagger"),
            new Troll("Morg", 4, 60, "club"),
            new Archer("Robin", 2, 30, "longbow|quiver"),
            new Healer("Mira", 2, 18, "staff|bandages"),
            new Paladin("Aldric", 3, 32, "sword|shield|holy symbol")
        };

        var engine = new GameEngine(_fileHandler, entities);
        engine.RunTurn();

        Console.WriteLine("\nPress any key to return to the menu...");
        Console.ReadKey();
        Console.Clear();
    }
}
