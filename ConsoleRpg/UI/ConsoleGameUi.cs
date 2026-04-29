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
        Console.WriteLine("--- Characters ---");
        Console.WriteLine(" 1. Display Characters");
        Console.WriteLine(" 2. Select Character");
        Console.WriteLine(" 3. Add Character");
        Console.WriteLine(" 4. Edit Character");
        Console.WriteLine(" 5. Delete Character");
        Console.WriteLine(" 6. Level Up Character");
        Console.WriteLine(" 7. Character Detail");
        Console.WriteLine("--- World ---");
        Console.WriteLine(" 8. Display Rooms");
        Console.WriteLine(" 9. Add Room");
        Console.WriteLine("10. Connect Rooms (Add Door)");
        Console.WriteLine("11. Display Current Room");
        Console.WriteLine("12. Move Player");
        Console.WriteLine("--- Items ---");
        Console.WriteLine("13. Add Item");
        Console.WriteLine("--- Inventory (Player) ---");
        Console.WriteLine("14. Inventory Management");
        Console.WriteLine("--- Chests & Loot (Player, W13) ---");
        Console.WriteLine("15. Chest Interaction");
        Console.WriteLine(" 0. Exit");
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
