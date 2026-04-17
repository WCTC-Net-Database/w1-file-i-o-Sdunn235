using ConsoleRpg;

Startup.Initialize();
Startup.GameUi.DisplayWelcome();

bool running = true;
while (running)
{
    string choice = Startup.GameUi.GetMenuChoice();

    switch (choice)
    {
        // Characters
        case "1":
            Startup.GameEngine.DisplayCharacters();
            break;
        case "2":
            Startup.GameEngine.FindCharacter();
            break;
        case "3":
            Startup.GameEngine.AddCharacter();
            break;
        case "4":
            Startup.GameEngine.LevelUpCharacter();
            break;
        case "5":
            Startup.GameEngine.DisplayCharacterDetail();
            break;

        // World
        case "6":
            Startup.GameEngine.DisplayRooms();
            break;
        case "7":
            Startup.GameEngine.AddRoom();
            break;
        case "8":
            Startup.GameEngine.AddDoor();
            break;
        case "9":
            Startup.GameEngine.DisplayCurrentRoom();
            break;
        case "10":
            Startup.GameEngine.MovePlayer();
            break;

        // Equipment
        case "11":
            Startup.GameEngine.DisplayEquipment();
            break;
        case "12":
            Startup.GameEngine.EquipItem();
            break;

        // Items
        case "13":
            Startup.GameEngine.AddItem();
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
