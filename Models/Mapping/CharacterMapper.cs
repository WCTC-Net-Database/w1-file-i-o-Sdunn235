using W6SolidDip.Models.Characters;
using W6SolidDip.Models.DataTransfer;

namespace W6SolidDip.Models.Mapping;

/// <summary>
/// Maps between Data Transfer Objects (DTOs) and domain model objects.
/// This separation allows:
/// - Domain models to be abstract (can't be instantiated by serializers)
/// - File format to change without affecting domain logic
/// - Domain logic to change without affecting file format
/// 
/// This follows the Single Responsibility Principle (SRP):
/// - DTOs handle serialization
/// - Domain models handle business logic
/// - Mapper handles translation between the two
/// </summary>
public static class CharacterMapper
{
    /// <summary>
    /// Converts a DTO from file storage into a concrete Character domain object.
    /// </summary>
    /// <param name="dto">The data transfer object from file storage.</param>
    /// <returns>A BasicCharacter instance (concrete implementation of abstract Character).</returns>
    public static Character ToCharacter(CharacterDto dto)
    {
        return new BasicCharacter(
            dto.Name,
            dto.Class,
            dto.Level,
            dto.Hp,
            dto.Equipment
        );
    }

    /// <summary>
    /// Converts a domain Character object into a DTO for file storage.
    /// </summary>
    /// <param name="character">The domain character object.</param>
    /// <returns>A DTO ready for serialization.</returns>
    public static CharacterDto ToDto(Character character)
    {
        return new CharacterDto
        {
            Name = character.Name,
            Class = character.Class,
            Level = character.Level,
            Hp = character.Hp,
            Equipment = character.Equipment
        };
    }

    /// <summary>
    /// Converts a list of DTOs to domain objects.
    /// </summary>
    public static List<Character> ToCharacters(List<CharacterDto> dtos)
    {
        return dtos.Select(ToCharacter).ToList();
    }

    /// <summary>
    /// Converts a list of domain objects to DTOs.
    /// </summary>
    public static List<CharacterDto> ToDtos(List<Character> characters)
    {
        return characters.Select(ToDto).ToList();
    }
}
