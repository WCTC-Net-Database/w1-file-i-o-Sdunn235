using ConsoleRpgEntities.Models;

namespace ConsoleRpg.UI;

/// <summary>
/// Contract for all game UI interactions.
/// OCP: Swap ConsoleGameUi for WebGameUi or TestGameUi with zero engine changes.
/// DIP: GameEngine depends on this abstraction — never on ConsoleGameUi directly.
/// </summary>
public interface IGameUi
{
    void DisplayWelcome();
    string GetMenuChoice();
    void DisplayPlayer(Player player);
    void DisplayMonsters(IEnumerable<MonsterBase> monsters);
    void DisplayCombatResult(string result);
    void DisplayMessage(string message);
    void PauseAndClear();

    /// <summary>Asks the player if they want to reset the battle. Returns true if yes.</summary>
    bool AskResetBattle();
}
