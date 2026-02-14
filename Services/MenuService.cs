namespace W3Srp.Services;

/// <summary>
/// Responsible for displaying menus and getting user choices.
/// This class follows the Single Responsibility Principle - it only handles menu display and navigation.
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
        Console.WriteLine("Week 3: Single Responsibility Principle\n");
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
}
