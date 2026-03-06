using W6DependencyInversion.Interfaces;
using W6DependencyInversion.Models.Characters;

namespace W6DependencyInversion.Services;

/// <summary>
/// Responsible for all character-related console interactions.
/// Depends on IFileHandler (abstraction) and CharacterBase (abstraction) - DIP compliant.
/// </summary>
public class CharacterUI
{
    private readonly IFileHandler _fileHandler;

    /// <summary>
    /// Initializes a new instance of CharacterUI with a file handler.
    /// </summary>
    /// <param name="fileHandler">The file handler implementation (CSV, JSON, etc.).</param>
    public CharacterUI(IFileHandler fileHandler)
    {
        _fileHandler = fileHandler;
    }

    /// <summary>
    /// Displays all characters from the data source in a formatted list.
    /// </summary>
    public void DisplayAllCharacters()
    {
        Console.WriteLine("\n=== All Characters ===\n");

        List<CharacterBase> characters = _fileHandler.ReadAll();

        if (characters.Count == 0)
        {
            Console.WriteLine("No characters found.");
            return;
        }

        foreach (CharacterBase character in characters)
        {
            DisplayCharacterDetails(character);
            Console.WriteLine("-------------------------");
        }

        Console.WriteLine($"Total characters: {characters.Count}");
    }

    /// <summary>
    /// Prompts user for a character name and displays the matching character if found.
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

        List<CharacterBase> characters = _fileHandler.ReadAll();
        CharacterBase? character = _fileHandler.FindByName(characters, name);

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

        CharacterBase newCharacter = new Character
        {
            Name = name ?? string.Empty,
            Class = characterClass ?? string.Empty,
            Level = int.TryParse(level, out int lvl) ? lvl : 1,
            Hp = int.TryParse(hp, out int health) ? health : 0,
            Equipment = equipment ?? string.Empty
        };

        _fileHandler.AppendCharacter(newCharacter);

        Console.WriteLine($"\nCharacter '{name}' has been added successfully!");
    }

    /// <summary>
    /// Prompts user for a character name and increases their level by 1 if found.
    /// </summary>
    public void LevelUpCharacter()
    {
        Console.WriteLine("\n=== Level Up Character ===\n");

        Console.Write("Enter character name to level up: ");
        string? nameToFind = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(nameToFind))
        {
            Console.WriteLine("Invalid name entered.");
            return;
        }

        List<CharacterBase> characters = _fileHandler.ReadAll();
        bool characterFound = false;

        foreach (CharacterBase character in characters)
        {
            if (character.Name.Equals(nameToFind, StringComparison.OrdinalIgnoreCase))
            {
                characterFound = true;
                int currentLevel = character.Level;
                character.Level = currentLevel + 1;
                Console.WriteLine($"\n'{character.Name}' has been leveled up from Level {currentLevel} to Level {character.Level}!");
            }
        }

        if (!characterFound)
        {
            Console.WriteLine($"\nCharacter '{nameToFind}' not found.");
            return;
        }

        _fileHandler.WriteAll(characters);
    }

    /// <summary>
    /// Helper method to display a single character's details.
    /// </summary>
    private static void DisplayCharacterDetails(CharacterBase character)
    {
        Console.WriteLine($"Name: {character.Name}");
        Console.WriteLine($"Class: {character.Class}");
        Console.WriteLine($"Level: {character.Level}");
        Console.WriteLine($"HP: {character.Hp}");

        List<string> equipmentItems = character.GetEquipmentList();
        Console.WriteLine($"Equipment: {string.Join(", ", equipmentItems)}");
    }
}
