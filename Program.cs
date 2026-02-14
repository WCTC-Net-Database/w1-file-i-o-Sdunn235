using W3Srp.Services;

/// <summary>
/// Week 3: Single Responsibility Principle - Console RPG Character Manager
///
/// This program demonstrates the Single Responsibility Principle (SRP) by separating concerns:
/// - CharacterReader: Handles reading and searching character data
/// - CharacterWriter: Handles writing character data
/// - CharacterUI: Handles all character-related console interactions
/// - MenuService: Handles menu display and navigation
/// - Program: Orchestrates the application flow (main loop only)
///
/// This refactoring shows how SRP creates clean, maintainable, and testable code.
/// Each class has ONE reason to change.
/// </summary>
///
/// <remarks>
/// *Disclosure*: This project was initially set up using AI assistance as part of a Week 2 learning exercise.
/// The code has since been reviewed, tested, and modified to ensure it works as intended.
/// Moving forward, testing and modifications will be self-directed.
/// </remarks>

class Program
{
    static void Main()
    {
        // Set up services following Dependency Injection pattern
        const string filePath = "input.csv";
        CharacterReader reader = new(filePath);
        CharacterWriter writer = new(filePath);
        CharacterUI characterUI = new(reader, writer);
        MenuService menu = new();

        // Display welcome message
        menu.DisplayWelcome();

        // Main program loop - keeps running until user chooses to exit
        bool running = true;
        while (running)
        {
            // Display the menu and get user's choice
            menu.DisplayMainMenu();
            string choice = menu.GetMenuChoice();

            // Process the user's choice using a switch statement
            switch (choice)
            {
                case "1":
                    characterUI.DisplayAllCharacters();
                    break;
                case "2":
                    characterUI.FindCharacter();
                    break;
                case "3":
                    characterUI.AddCharacter();
                    break;
                case "4":
                    characterUI.LevelUpCharacter();
                    break;
                case "0":
                    running = false;
                    menu.DisplayGoodbye();
                    break;
                default:
                    menu.DisplayInvalidChoice();
                    break;
            }

            // Pause and clear screen between operations
            if (running)
            {
                menu.PauseAndClear();
            }
        }
    }
}
