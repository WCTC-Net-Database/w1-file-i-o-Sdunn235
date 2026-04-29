using ConsoleRpg;

Startup.Initialize();
Startup.GameUi.DisplayWelcome();

bool running = true;
while (running)
{
    string choice = Startup.GameUi.GetMenuChoice(Startup.GameEngine.ActiveCharacterLabel);

    switch (choice)
    {
        // Characters
        case "1":
            Startup.GameEngine.DisplayCharacters();
            break;
        case "2":
            Startup.GameEngine.SelectCharacter();
            break;
        case "3":
            Startup.GameEngine.AddCharacter();
            break;
        case "4":
            Startup.GameEngine.EditCharacter();
            break;
        case "5":
            Startup.GameEngine.DeleteCharacter();
            break;
        case "6":
            Startup.GameEngine.LevelUpCharacter();
            break;
        case "7":
            Startup.GameEngine.DisplayCharacterDetail();
            break;

        // World
        case "8":
            Startup.GameEngine.DisplayRooms();
            break;
        case "9":
            Startup.GameEngine.AddRoom();
            break;
        case "10":
            Startup.GameEngine.AddDoor();
            break;
        case "11":
            Startup.GameEngine.DisplayCurrentRoom();
            break;
        case "12":
            Startup.GameEngine.MovePlayer();
            break;

        // Items
        case "13":
            Startup.GameEngine.AddItem();
            break;

        // Inventory Management (W12)
        case "14":
            Startup.GameEngine.InventoryMenu();
            break;

        // Chest Interaction (W13)
        case "15":
            Startup.GameEngine.ChestMenu();
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
