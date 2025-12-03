using ConsoleRpg.Helpers;
using ConsoleRpgEntities.Data;
using ConsoleRpgEntities.Models.Attributes;
using ConsoleRpgEntities.Models.Characters;
using ConsoleRpgEntities.Models.Characters.Monsters;
using Microsoft.EntityFrameworkCore;

namespace ConsoleRpg.Services;

public class GameEngine
{
    private readonly GameContext _context;
    private readonly MenuManager _menuManager;
    private readonly OutputManager _outputManager;

    private IPlayer? _player;
    private IMonster? _goblin;

    public GameEngine(GameContext context, MenuManager menuManager, OutputManager outputManager)
    {
        _menuManager = menuManager;
        _outputManager = outputManager;
        _context = context;
    }

    public void Run()
    {
        if (_menuManager.ShowMainMenu())
        {
            SetupGame();
        }
    }

    private void GameLoop()
    {
        _outputManager.Clear();

        while (true)
        {
            _outputManager.WriteLine("Choose an action:", ConsoleColor.Cyan);
            _outputManager.WriteLine("1. Attack", ConsoleColor.Cyan);
            _outputManager.WriteLine("2. Quit", ConsoleColor.Cyan);
            _outputManager.WriteLine("3. Inventory", ConsoleColor.Cyan);

            _outputManager.Display();

            var input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    AttackCharacter();
                    break;
                case "2":
                    _outputManager.WriteLine("Exiting game...", ConsoleColor.Red);
                    _outputManager.Display();
                    Environment.Exit(0);
                    break;
                case "3":
                    // Show inventory menu during gameplay
                    try
                    {
                        _menuManager.ShowInventoryMenu(_player!, this);
                    }
                    catch (Exception ex)
                    {
                        _outputManager.WriteLine($"Unable to open inventory: {ex.Message}", ConsoleColor.Yellow);
                        _outputManager.Display();
                    }
                    break;
                default:
                    _outputManager.WriteLine("Invalid selection. Please choose 1.", ConsoleColor.Red);
                    break;
            }
        }
    }

    // Public API used by MenuManager to query world items and add them to the player's inventory
    public IEnumerable<ConsoleRpgEntities.Models.Equipments.Item> FindWorldItems(string namePart)
    {
        if (string.IsNullOrWhiteSpace(namePart))
        {
            // return first 50 as a sample
            return _context.Items.Take(50).ToList();
        }

        // Normalize to lower-case for case-insensitive search in the database.
        var search = namePart.Trim().ToLowerInvariant();
        return _context.Items
            .Where(i => i.Name != null && i.Name.ToLower().Contains(search))
            .ToList();
    }

    public bool TryAddItemToPlayerById(int itemId, out string message)
    {
        message = string.Empty;
        var item = _context.Items.Find(itemId);
        if (item == null)
        {
            message = "Item not found in the world.";
            return false;
        }

        if (_player == null)
        {
            message = "Player not loaded.";
            return false;
        }

        // Ensure inventory exists
        var player = _player as ConsoleRpgEntities.Models.Characters.Player;
        if (player == null)
        {
            message = "Player type does not support inventory operations.";
            return false;
        }

        player.Inventory ??= new ConsoleRpgEntities.Models.Equipments.Inventory { PlayerId = player.Id, Player = player, Items = new List<ConsoleRpgEntities.Models.Equipments.Item>() };
        player.Inventory.Items ??= new List<ConsoleRpgEntities.Models.Equipments.Item>();

        if (!player.CanAddItem(item))
        {
            message = "Cannot add item; it would exceed MaxWeight.";
            return false;
        }

        // Attach item to context (if not already) and add to player's inventory
        var trackedItem = _context.Items.Find(item.Id) ?? item;
        player.Inventory.Items.Add(trackedItem);

        // Persist the association: if your schema uses a join or PlayerId on Item, you may need additional mapping.
        _context.SaveChanges();
        message = "Item added to inventory.";
        return true;
    }

    private void AttackCharacter()
    {
        if (_goblin is ITargetable targetableGoblin && _player != null)
        {
            _player.Attack(targetableGoblin);
            if (_player.Abilities != null && _player.Abilities.Any())
            {
                _player.UseAbility(_player.Abilities.First(), targetableGoblin);
            }
        }
    }

    private void SetupGame()
    {
        // Load the player with inventory/items, equipment and abilities so inventory features work
        _player = _context.Players
            .Include(p => p.Inventory)
                .ThenInclude(inv => inv.Items)
            .Include(p => p.Equipment)
            .Include(p => p.Abilities)
            .FirstOrDefault();

        if (_player == null)
        {
            _outputManager.WriteLine("No player found in the database. Ensure you have seeded a player.", ConsoleColor.Red);
            _outputManager.Display();
            Environment.Exit(1);
        }

        _outputManager.WriteLine($"{_player.Name} has entered the game.", ConsoleColor.Green);

        // Load monsters into random rooms 
        LoadMonsters();

        // Pause before starting the game loop
        Thread.Sleep(500);
        GameLoop();
    }

    private void LoadMonsters()
    {
        _goblin = _context.Monsters.OfType<Goblin>().FirstOrDefault();
    }

}
