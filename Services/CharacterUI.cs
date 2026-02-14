namespace W3Srp.Services;

/// <summary>
/// Responsible for all character-related console interactions.
/// This class follows the Single Responsibility Principle - it only handles UI for character operations.
/// </summary>
public class CharacterUI
{
    private readonly CharacterReader _reader;
    private readonly CharacterWriter _writer;

    /// <summary>
    /// Initializes a new instance of CharacterUI with required services.
    /// </summary>
    /// <param name="reader">The CharacterReader service for reading character data.</param>
    /// <param name="writer">The CharacterWriter service for writing character data.</param>
    public CharacterUI(CharacterReader reader, CharacterWriter writer)
    {
        _reader = reader;
        _writer = writer;
    }

    /// <summary>
    /// Displays all characters from the data source in a formatted list.
    /// </summary>
    public void DisplayAllCharacters()
    {
        Console.WriteLine("\n=== All Characters ===\n");
        
        List<Character> characters = _reader.ReadAll();

        if (characters.Count == 0)
        {
            Console.WriteLine("No characters found.");
            return;
        }

        // Display each character's information with clear formatting
        foreach (Character character in characters)
        {
            DisplayCharacterDetails(character);
            Console.WriteLine("-------------------------");
        }
        
        Console.WriteLine($"Total characters: {characters.Count}");
    }

    /// <summary>
    /// Prompts user for a character name and displays the matching character if found.
    /// Uses LINQ through the CharacterReader service.
    /// </summary>
    public void FindCharacter()
    {
        Console.WriteLine("\n=== Find Character ===\n");
        
        Console.Write("Enter character name to find: ");
        string? name = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(name))
        {
            Console.WriteLine("Invalid name entered.");
            return;
        }

        // Use LINQ through CharacterReader service
        Character? character = _reader.FindByName(name);

        if (character == null)
        {
            Console.WriteLine($"\nCharacter '{name}' not found.");
        }
        else
        {
            Console.WriteLine("\nCharacter found:");
            DisplayCharacterDetails(character);
        }
    }

    /// <summary>
    /// Prompts user for character information and adds a new character to the data source.
    /// </summary>
    public void AddCharacter()
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

        // Create new character object with input validation
        Character newCharacter = new()
        {
            Name = name ?? string.Empty,
            Class = characterClass ?? string.Empty,
            Level = int.TryParse(level, out int lvl) ? lvl : 1,
            Hp = int.TryParse(hp, out int health) ? health : 0,
            Equipment = equipment ?? string.Empty
        };

        // Use CharacterWriter service to append the new character
        _writer.AppendCharacter(newCharacter);

        Console.WriteLine($"\nCharacter '{name}' has been added successfully!");
    }

    /// <summary>
    /// Prompts user for a character name and increases their level by 1 if found.
    /// </summary>
    public void LevelUpCharacter()
    {
        Console.WriteLine("\n=== Level Up Character ===\n");

        // Get the character name to find and level up
        Console.Write("Enter character name to level up: ");
        string? nameToFind = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(nameToFind))
        {
            Console.WriteLine("Invalid name entered.");
            return;
        }

        // Load all character records using CharacterReader
        List<Character> characters = _reader.ReadAll();
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

        // Write the modified list back to file using CharacterWriter
        _writer.WriteAll(characters);
    }

    /// <summary>
    /// Helper method to display a single character's details.
    /// </summary>
    /// <param name="character">The character to display.</param>
    private void DisplayCharacterDetails(Character character)
    {
        Console.WriteLine($"Name: {character.Name}");
        Console.WriteLine($"Class: {character.Class}");
        Console.WriteLine($"Level: {character.Level}");
        Console.WriteLine($"HP: {character.Hp}");

        // Use the Character's method to get equipment list
        List<string> equipmentItems = character.GetEquipmentList();
        Console.WriteLine($"Equipment: {string.Join(", ", equipmentItems)}");
    }
}
