namespace ConsoleRpg.UI;

public interface IGameUi
{
    void DisplayWelcome();
    string GetMenuChoice();
    void DisplayMessage(string message);
    void PauseAndClear();
}
