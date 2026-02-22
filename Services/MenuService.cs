namespace W4Ocp.Services;

/// <summary>
/// Responsible for displaying menus and getting user choices.
/// This class follows the Single Responsibility Principle and Open/Closed Principle.
/// </summary>
public class MenuService
{
    /// <summary>
    /// Displays the main menu options to the user.
    /// </summary>
    public void DisplayMainMenu()
    {
        Console.WriteLine("What would you like to do?");
        Console.WriteLine("1. Display All Characters");
        Console.WriteLine("2. Find Character");
        Console.WriteLine("3. Add New Character");
        Console.WriteLine("4. Level Up Character");
        Console.WriteLine("5. Switch File Format (CSV/JSON)");
        Console.WriteLine("0. Exit");
    }

    /// <summary>
    /// Gets the user's menu choice from console input.
    /// </summary>
    /// <returns>The user's menu choice as a string.</returns>
    public string GetMenuChoice()
    {
        Console.Write("\nEnter your choice: ");
        return Console.ReadLine() ?? string.Empty;
    }

    /// <summary>
    /// Displays a welcome message when the program starts.
    /// </summary>
    public void DisplayWelcome()
    {
        Console.WriteLine("=== Console RPG Character Manager ===");
        Console.WriteLine("Week 4: Open/Closed Principle\n");
    }

    /// <summary>
    /// Displays a goodbye message when the user exits.
    /// </summary>
    public void DisplayGoodbye()
    {
        Console.WriteLine("\nGoodbye! Thanks for playing.");
    }

    /// <summary>
    /// Displays an invalid choice message.
    /// </summary>
    public void DisplayInvalidChoice()
    {
        Console.WriteLine("\nInvalid choice. Please try again.");
    }

    /// <summary>
    /// Pauses execution and waits for user to press a key, then clears the console.
    /// </summary>
    public void PauseAndClear()
    {
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
        Console.Clear();
    }

    /// <summary>
    /// Displays the file format selection menu and gets the user's choice.
    /// </summary>
    /// <returns>The selected format as a string ("csv" or "json"), or empty string if cancelled.</returns>
    public string GetFileFormatChoice()
    {
        Console.WriteLine("\n=== Switch File Format ===");
        Console.WriteLine("1. CSV Format");
        Console.WriteLine("2. JSON Format");
        Console.WriteLine("0. Cancel");
        Console.Write("\nEnter your choice: ");
        
        string choice = Console.ReadLine() ?? string.Empty;
        
        return choice switch
        {
            "1" => "csv",
            "2" => "json",
            "0" => string.Empty,
            _ => string.Empty
        };
    }

    /// <summary>
    /// Displays a success message when the file format is changed.
    /// </summary>
    /// <param name="format">The new file format (CSV or JSON).</param>
    public void DisplayFormatChanged(string format)
    {
        Console.WriteLine($"\n✓ File format successfully changed to {format.ToUpper()}");
    }

    /// <summary>
    /// Displays a cancellation message when format switch is cancelled.
    /// </summary>
    public void DisplayFormatChangeCancelled()
    {
        Console.WriteLine("\nFormat switch cancelled.");
    }
}
