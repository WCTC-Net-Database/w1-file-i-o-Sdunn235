/// *Disclosure*: This project was initially set up using AI assistance as part of a Week 2 learning exercise.
/// The code has since been reviewed, tested, and modified to ensure it works as intended.
/// Moving forward, testing and modifications will be self-directed.

using System.Globalization;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;

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

    // CsvHelper configuration: no header row in the CSV
    static readonly CsvConfiguration CsvConfig = new(CultureInfo.InvariantCulture)
    {
        HasHeaderRecord = false,
        TrimOptions = TrimOptions.Trim
    };

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
        List<Character> characters = ReadCharacters();

        // Display each character's information with clear formatting
        foreach (Character character in characters)
        {
            Console.WriteLine($"Name: {character.Name}");
            Console.WriteLine($"Class: {character.Class}");
            Console.WriteLine($"Level: {character.Level}");
            Console.WriteLine($"HP: {character.Hp}");

            // Split equipment by '|' pipe character and rejoin with commas for display
            string[] equipmentItems = character.Equipment.Split('|');
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

        // Load existing records, append the new one, and write the file back out
        List<Character> characters = ReadCharacters();
        characters.Add(new Character
        {
            Name = name ?? string.Empty,
            Class = characterClass ?? string.Empty,
            Level = int.TryParse(level, out int lvl) ? lvl : 1,
            Hp = int.TryParse(hp, out int health) ? health : 0,
            Equipment = equipment ?? string.Empty
        });

        WriteCharacters(characters);

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

        // Load all character records from file into a modifiable list
        List<Character> characters = ReadCharacters();
        bool characterFound = false;

        foreach (Character character in characters)
        {
            // Compare character names (case-insensitive for better UX)
            if (character.Name.Equals(nameToFind, StringComparison.OrdinalIgnoreCase))
            {
                characterFound = true;
                int currentLevel = character.Level;
                character.Level = currentLevel + 1;
                Console.WriteLine($"\n'{character.Name}' has been leveled up from Level {currentLevel} to Level {character.Level}!");
            }
        }

        // If character was not found, inform user and exit early
        if (!characterFound)
        {
            Console.WriteLine($"\nCharacter '{nameToFind}' not found.");
            return;
        }

        // Write the modified list back to file, permanently saving the change
        WriteCharacters(characters);
    }

    /// <summary>
    /// Reads all characters from the CSV file using CsvHelper.
    /// </summary>
    static List<Character> ReadCharacters()
    {
        if (!File.Exists(filePath))
        {
            return new List<Character>();
        }

        using StreamReader reader = new(filePath);
        using CsvReader csv = new(reader, CsvConfig);
        csv.Context.RegisterClassMap<CharacterMap>();
        return csv.GetRecords<Character>().ToList();
    }

    /// <summary>
    /// Writes all characters to the CSV file using CsvHelper.
    /// </summary>
    static void WriteCharacters(IEnumerable<Character> characters)
    {
        using StreamWriter writer = new(filePath, false);
        using CsvWriter csv = new(writer, CsvConfig);
        csv.Context.RegisterClassMap<CharacterMap>();
        csv.WriteRecords(characters);
    }
}

class Character
{
    public string Name { get; set; } = string.Empty;
    public string Class { get; set; } = string.Empty;
    public int Level { get; set; }
    public int Hp { get; set; }
    public string Equipment { get; set; } = string.Empty;
}

sealed class CharacterMap : ClassMap<Character>
{
    public CharacterMap()
    {
        // CsvHelper uses these mappings to match CSV column positions to Character properties.
        // Index(0) is the first column, Index(1) is the second, etc. This is used for both read and write.
        Map(m => m.Name).Index(0);
        Map(m => m.Class).Index(1);
        Map(m => m.Level).Index(2);
        Map(m => m.Hp).Index(3);
        Map(m => m.Equipment).Index(4);
    }
}
