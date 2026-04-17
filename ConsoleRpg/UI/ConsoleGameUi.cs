namespace ConsoleRpg.UI;

public class ConsoleGameUi : IGameUi
{
    public void DisplayWelcome()
    {
        Console.Clear();
        Console.WriteLine("╔══════════════════════════════════════════╗");
        Console.WriteLine("║       W11 Console RPG — LucentForge     ║");
        Console.WriteLine("║   Equipment & Room Navigation            ║");
        Console.WriteLine("╚══════════════════════════════════════════╝");
        Console.WriteLine();
    }

    public string GetMenuChoice()
    {
        Console.WriteLine("=== Main Menu ===");
        Console.WriteLine("--- Characters ---");
        Console.WriteLine(" 1. Display Characters");
        Console.WriteLine(" 2. Find Character");
        Console.WriteLine(" 3. Add Character");
        Console.WriteLine(" 4. Level Up Character");
        Console.WriteLine(" 5. Character Detail");
        Console.WriteLine("--- World ---");
        Console.WriteLine(" 6. Display Rooms");
        Console.WriteLine(" 7. Add Room");
        Console.WriteLine(" 8. Connect Rooms (Add Door)");
        Console.WriteLine(" 9. Display Current Room");
        Console.WriteLine("10. Move Player");
        Console.WriteLine("--- Equipment ---");
        Console.WriteLine("11. Display Equipment");
        Console.WriteLine("12. Equip Item");
        Console.WriteLine("--- Items ---");
        Console.WriteLine("13. Add Item");
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
        Console.ReadKey();
        Console.Clear();
    }
}
