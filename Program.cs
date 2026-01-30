/// <summary>
/// *Disclosure*: This project was initially set up using AI assistance as part of a Week 2 learning exercise.
/// The code has since been reviewed, tested, and modified to ensure it works as intended.
/// Moving forward, testing and modifications will be self-directed.
/// Note: In real-world scenarios, development often involves modifying existing codebases rather than writing from scratch.
/// </summary>


/// <summary>
/// Week 1: File I/O Basics - Console RPG Character Manager
///
/// This program demonstrates fundamental file operations in C#:
/// - Reading data from CSV files using File.ReadAllLines()
/// - Parsing comma-separated values using String.Split()
/// - Writing and appending data to files using File.WriteAllLines() and File.AppendAllText()
///
/// Features implemented:
/// 1. Display all characters from CSV file with formatted output
/// 2. Add new characters by collecting user input and appending to file
/// 3. Level up existing characters by modifying and rewriting file data
/// </summary>

/// *Disclosure*: The basis of this code was created by an AI language model VS Code Claude Haiku 4.5.
/// It has been reviewed and enhanced by a human developer to ensure accuracy and clarity.

class Program
{
    // The path to our data file - we'll read and write character data here
    static string filePath = "input.csv";

    static void Main()
    {
        // Welcome message
        Console.WriteLine("=== Console RPG Character Manager ===");
        Console.WriteLine("Week 1: File I/O Basics\n");

        // Main program loop - keeps running until user chooses to exit
        bool running = true;
        while (running)
        {
            // Display the menu options
            DisplayMenu();

            // Get user's choice
            Console.Write("\nEnter your choice: ");
            string? choice = Console.ReadLine();

            // Process the user's choice using a switch statement
            switch (choice)
            {
                case "1":
                    DisplayAllCharacters();
                    break;
                case "2":
                    AddCharacter();
                    break;
                case "3":
                    LevelUpCharacter();
                    break;
                case "0":
                    running = false;
                    Console.WriteLine("\nGoodbye! Thanks for playing.");
                    break;
                default:
                    Console.WriteLine("\nInvalid choice. Please try again.");
                    break;
            }

            // Add a blank line for readability between menu displays
            if (running)
            {
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                Console.Clear();
            }
        }
    }

    /// <summary>
    /// Displays the main menu options to the user.
    /// This is complete - review it to understand the structure.
    /// </summary>
    static void DisplayMenu()
    {
        Console.WriteLine("What would you like to do?");
        Console.WriteLine("1. Display All Characters");
        Console.WriteLine("2. Add New Character");
        Console.WriteLine("3. Level Up Character");
        Console.WriteLine("0. Exit");
    }

    /// <summary>
    /// Reads all characters from the CSV file and displays them in a formatted list.
    /// 
    /// Implementation:
    /// - File.ReadAllLines() loads entire CSV file into a string array
    /// - Each line is split by comma using Split(',') to extract individual fields
    /// - Fields are indexed: Name(0), Class(1), Level(2), HP(3), Equipment(4)
    /// - Equipment items are further split by '|' and joined with commas for readability
    /// - Each character is displayed with formatted console output
    /// </summary>
    static void DisplayAllCharacters()
    {
        // Read all lines from the file into a string array
        string[] lines = File.ReadAllLines(filePath);

        // Loop through each line and parse the CSV data
        foreach (string line in lines)
        {
            // Split() separates the CSV line by commas into individual field values
            string[] parts = line.Split(',');

            // Extract each field from the array using index positions
            string name = parts[0];
            string characterClass = parts[1];
            string level = parts[2];
            string hp = parts[3];
            string equipment = parts[4];

            // Display each character's information with clear formatting
            Console.WriteLine($"Name: {name}");
            Console.WriteLine($"Class: {characterClass}");
            Console.WriteLine($"Level: {level}");
            Console.WriteLine($"HP: {hp}");

            // Split equipment by '|' pipe character and rejoin with commas for display
            string[] equipmentItems = equipment.Split('|');
            Console.WriteLine($"Equipment: {string.Join(", ", equipmentItems)}");
            Console.WriteLine("-------------------------");
        }
    }

    /// <summary>
    /// Prompts the user for character information and appends it to the CSV file.
    /// 
    /// Implementation:
    /// - Collects Name, Class, Level, HP, and Equipment from user via Console.ReadLine()
    /// - Uses string interpolation ($"") to format the CSV line
    /// - File.AppendAllText() adds the new line without overwriting existing data
    /// - Adds newline character (\n) to separate records in the file
    /// </summary>
    static void AddCharacter()
    {
        Console.WriteLine("\n=== Add New Character ===\n");

        // Collect all character information from user input
        Console.Write("Enter character name: ");
        string? name = Console.ReadLine();

        Console.Write("Enter character class: ");
        string? characterClass = Console.ReadLine();

        Console.Write("Enter character level: ");
        string? level = Console.ReadLine();

        Console.Write("Enter character HP: ");
        string? hp = Console.ReadLine();

        Console.Write("Enter equipment (separated by |, e.g., sword|shield): ");
        string? equipment = Console.ReadLine();

        // Format all fields as a single CSV line using string interpolation
        string newLine = $"{name},{characterClass},{level},{hp},{equipment}";

        // Append the new character line to the file with a newline separator
        // AppendAllText() preserves existing data instead of overwriting it
        // Add newline BEFORE the new character to ensure it starts on a fresh line
        File.AppendAllText(filePath, "\n" + newLine);

        Console.WriteLine($"\nCharacter '{name}' has been added successfully!");
    }

    /// <summary>
    /// Finds a character by name, increases their level by 1, and saves changes to file.
    /// 
    /// Implementation:
    /// - Prompts user to enter the character name to level up
    /// - File.ReadAllLines() loads all character records into a modifiable array
    /// - Loops through array with index-based loop to enable modification
    /// - Uses case-insensitive comparison (StringComparison.OrdinalIgnoreCase) for user-friendly search
    /// - Parses the matching line, converts level to int, increments by 1
    /// - Rebuilds the CSV line with updated level and stores in original array position
    /// - File.WriteAllLines() overwrites the file with the modified array
    /// </summary>
    static void LevelUpCharacter()
    {
        Console.WriteLine("\n=== Level Up Character ===\n");

        // Get the character name to find and level up
        Console.Write("Enter character name to level up: ");
        string? nameToFind = Console.ReadLine();

        // Load all character records from file into a modifiable string array
        string[] lines = File.ReadAllLines(filePath);
        bool characterFound = false;

        // Use index-based loop to enable modification of array elements
        for (int i = 0; i < lines.Length; i++)
        {
            // Split the CSV line to extract individual fields
            string[] parts = lines[i].Split(',');
            string name = parts[0];

            // Compare character names (case-insensitive for better UX)
            // Equals() with OrdinalIgnoreCase makes "john" match "John"
            if (name.Equals(nameToFind, StringComparison.OrdinalIgnoreCase))
            {
                characterFound = true;

                // Extract all fields from the parsed CSV line
                string characterClass = parts[1];
                // Parse level as integer to perform arithmetic operation
                int currentLevel = int.Parse(parts[2]);
                string hp = parts[3];
                string equipment = parts[4];

                // Increment level by 1
                int newLevel = currentLevel + 1;

                // Rebuild the CSV line with the updated level and store in array
                // This modifies the array in-memory without affecting the file yet
                lines[i] = $"{name},{characterClass},{newLevel},{hp},{equipment}";

                Console.WriteLine($"\n'{name}' has been leveled up from Level {currentLevel} to Level {newLevel}!");
            }
        }

        // If character was not found, inform user and exit early
        if (!characterFound)
        {
            Console.WriteLine($"\nCharacter '{nameToFind}' not found.");
            return;
        }

        // Write the modified array back to file, permanently saving the change
        // WriteAllLines() overwrites the entire file with the updated content
        File.WriteAllLines(filePath, lines);
    }
}
