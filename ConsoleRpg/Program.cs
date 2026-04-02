using ConsoleRpg;

/// <summary>
/// Week 9: EF Core Intro — Console RPG Entry Point
///
/// Program.cs is intentionally minimal. All dependency wiring lives in Startup.cs (DIP).
/// This file is the application entry point only — it starts the app and runs the main loop.
///
/// Menu options:
///   1. Start Combat        — W7 game loop (FileContext + BattleService + LINQ)
///   2. View Player         — display player stats and equipped items
///   3. Reset Battle        — restore monsters + heal player to full HP
///   4. Character Manager   — W6 CSV/JSON character manager (IFileHandler)
///   5. W6 GameEngine Demo  — W6 entity-turn demo (DIP + Abstract Classes)
///   --- W9: EF Core ---
///   6. Display Characters  — list all characters from SQL Server
///   7. Find Character      — LINQ search by name
///   8. Add Character       — create character with room association
///   9. Add Room            — create a new room
///  10. Level Up Character  — increment character level (stretch goal)
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
        // W7: File-backed features (FileContext / JSON)
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

        // W9: Database-backed features (GameContext / EF Core / SQL Server)
        case "6":
            Startup.GameEngine.DisplayCharacters();
            break;
        case "7":
            Startup.GameEngine.FindCharacter();
            break;
        case "8":
            Startup.GameEngine.AddCharacter();
            break;
        case "9":
            Startup.GameEngine.AddRoom();
            break;
        case "10":
            Startup.GameEngine.LevelUpCharacter();
            break;
        case "11":
            Startup.GameEngine.DisplayRooms();
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
