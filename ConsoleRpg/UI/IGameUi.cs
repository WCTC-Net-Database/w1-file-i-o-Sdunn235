namespace ConsoleRpg.UI;

public interface IGameUi
{
    void DisplayWelcome();
    string GetMenuChoice();
    string GetMenuChoice(string? activeLabel);
    void DisplayMessage(string message);
    void PauseAndClear();
}
