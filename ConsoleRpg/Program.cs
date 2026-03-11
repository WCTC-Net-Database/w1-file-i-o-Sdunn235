using ConsoleRpg;

/// <summary>
/// Week 7: Midterm Prep — W7 Console RPG Entry Point
///
/// Program.cs is intentionally minimal. All dependency wiring lives in Startup.cs (DIP).
/// This file is the application entry point only — it starts the app and runs the main loop.
///
/// Menu options:
///   1. Start Combat       — W7 game loop (IContext + BattleService + LINQ)
///   2. View Player        — display player stats and equipped items
///   3. Reset Battle       — restore monsters + heal player to full HP
///   4. Character Manager  — W6 CSV/JSON character manager (IFileHandler)
///   5. W6 GameEngine Demo — W6 entity-turn demo (DIP + Abstract Classes)
///   0. Exit
/// </summary>
/// <remarks>
/// *Disclosure*: This project was initially set up using AI assistance as part of a Week 2 learning exercise.
/// The code has since been reviewed, tested, and modified to ensure it works as intended.
/// </remarks>

Startup.Initialize();
Startup.GameUi.DisplayWelcome();

bool running = true;
while (running)
{
    string choice = Startup.GameUi.GetMenuChoice();

    switch (choice)
    {
        case "1":
            Startup.GameEngine.RunCombat();
            break;
        case "2":
            Startup.GameEngine.ViewPlayer();
            break;
        case "3":
            Startup.GameEngine.ResetBattle();
            break;
        case "4":
            Startup.RunCharacterManager();
            break;
        case "5":
            Startup.GameEngineDemo.Run();
            break;
        case "0":
            running = false;
            Startup.GameUi.DisplayMessage("Goodbye, adventurer!");
            break;
        default:
            Startup.GameUi.DisplayMessage("Invalid choice. Please try again.");
            break;
    }

    if (running)
        Startup.GameUi.PauseAndClear();
}
