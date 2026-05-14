namespace ConsoleRpg.UI;

public class ConsoleGameUi : IGameUi
{
    public void DisplayWelcome()
    {
        DrawBanner();
    }

    private static void DrawBanner()
    {
        Console.WriteLine("╔══════════════════════════════════════════╗");
        Console.WriteLine("║       W13 Console RPG — LucentForge     ║");
        Console.WriteLine("║      Chests, Monster Loot & Lockpicks   ║");
        Console.WriteLine("╚══════════════════════════════════════════╝");
        Console.WriteLine();
    }

    public string GetMenuChoice()
    {
        return GetMenuChoice(null);
    }

    public string GetMenuChoice(string? activeLabel)
    {
        Console.Clear();
        DrawBanner();

        if (!string.IsNullOrWhiteSpace(activeLabel))
            Console.WriteLine($"[Active: {activeLabel}]\n");

        Console.WriteLine("=== Main Menu ===");
        Console.WriteLine();
        Console.WriteLine(" -- Game --");
        Console.WriteLine("  7. Inventory      ▶");
        Console.WriteLine("  8. Chests & Loot  ▶");
        Console.WriteLine("  9. Bookshelves    ▶");
        Console.WriteLine();
        Console.WriteLine(" -- Admin --");
        Console.WriteLine("  1. Characters     ▶");
        Console.WriteLine("  2. Items          ▶");
        Console.WriteLine("  3. Rooms & Doors  ▶");
        Console.WriteLine("  4. Skills         ▶");
        Console.WriteLine("  5. Abilities      ▶");
        Console.WriteLine("  6. Magic          ▶");
        Console.WriteLine("  q. Queries        ▶");
        Console.WriteLine();
        Console.WriteLine("  0. Exit");
        Console.Write("\nEnter choice: ");
        return Console.ReadLine()?.Trim() ?? string.Empty;
    }

    public void DisplayMessage(string message)
    {
        Console.WriteLine($"\n{message}");
    }

    public void PauseAndClear()
    {
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey(intercept: true);
    }
}
