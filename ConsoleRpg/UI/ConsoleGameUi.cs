using ConsoleRpgEntities.Models;

namespace ConsoleRpg.UI;

/// <summary>
/// Console-based implementation of IGameUi.
/// All Console.WriteLine and Console.ReadLine calls for game display live here — nowhere else.
///
/// SRP: Display and input only — zero game logic.
/// OCP: A different UI (web, test, mobile) implements IGameUi without changing anything downstream.
/// </summary>
public class ConsoleGameUi : IGameUi
{
    public void DisplayWelcome()
    {
        Console.Clear();
        Console.WriteLine("╔══════════════════════════════════════════╗");
        Console.WriteLine("║          W9 Console RPG                  ║");
        Console.WriteLine("║   EF Core Intro — SOLID Architecture     ║");
        Console.WriteLine("╚══════════════════════════════════════════╝");
        Console.WriteLine();
    }

    public string GetMenuChoice()
    {
        Console.WriteLine("=== Main Menu ===");
        Console.WriteLine("1. Start Combat");
        Console.WriteLine("2. View Player");
        Console.WriteLine("3. Reset Battle");
        Console.WriteLine("4. Character Manager (W6)");
        Console.WriteLine("5. W6 GameEngine Demo");
        Console.WriteLine("--- W9: EF Core ---");
        Console.WriteLine("6. Display Characters");
        Console.WriteLine("7. Find Character");
        Console.WriteLine("8. Add Character");
        Console.WriteLine("9. Add Room");
        Console.WriteLine("10. Level Up Character");
        Console.WriteLine("11. Display Rooms");
        Console.WriteLine("0. Exit");
        Console.Write("\nEnter choice: ");
        return Console.ReadLine() ?? string.Empty;
    }

    public void DisplayPlayer(Player player)
    {
        Console.WriteLine("\n=== Player ===");
        Console.WriteLine(player);
        Console.WriteLine($"  Strength:     {player.AbilityScores.Strength}");
        Console.WriteLine($"  Dexterity:    {player.AbilityScores.Dexterity}");
        Console.WriteLine($"  Constitution: {player.AbilityScores.Constitution}");

        var equipped = player.Items.Where(i => i.IsEquipped).ToList();
        Console.WriteLine($"\n  Equipped Items ({equipped.Count}):");
        foreach (var item in equipped)
            Console.WriteLine($"    - {item}");
    }

    public void DisplayMonsters(IEnumerable<MonsterBase> monsters)
    {
        Console.WriteLine("\n=== Monsters ===");
        int i = 1;
        foreach (var monster in monsters)
            Console.WriteLine($"  {i++}. {monster}");
    }

    public void DisplayCombatResult(string result)
    {
        Console.WriteLine($"\n⚔  {result}");
    }

    public void DisplayMessage(string message)
    {
        Console.WriteLine($"\n{message}");
    }

    public void PauseAndClear()
    {
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
        Console.Clear();
    }

    public bool AskResetBattle()
    {
        Console.Write("\nReset battle? Restores full HP and all monsters. (Y/N): ");
        string input = Console.ReadLine()?.Trim().ToLower() ?? string.Empty;
        return input == "y" || input == "yes";
    }
}
