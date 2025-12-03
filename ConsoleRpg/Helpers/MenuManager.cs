using System;
using System.Linq;
using System.Collections.Generic;
using ConsoleRpgEntities.Models.Characters;
using ConsoleRpgEntities.Models.Equipments;

namespace ConsoleRpg.Helpers;

public class MenuManager
{
    private readonly OutputManager _outputManager;

    public MenuManager(OutputManager outputManager)
    {
        _outputManager = outputManager;
    }

    public bool ShowMainMenu()
    {
        _outputManager.WriteLine("Welcome to the RPG Game!", ConsoleColor.Yellow);
        _outputManager.WriteLine("1. Start Game", ConsoleColor.Cyan);
        _outputManager.WriteLine("2. Exit", ConsoleColor.Cyan);
        _outputManager.Display();

        return HandleMainMenuInput();
    }

    private bool HandleMainMenuInput()
    {
        while (true)
        {
            var input = Console.ReadLine();
            switch (input)
            {
                case "1":
                    _outputManager.WriteLine("Starting game...", ConsoleColor.Green);
                    _outputManager.Display();
                    return true;
                case "2":
                    _outputManager.WriteLine("Exiting game...", ConsoleColor.Red);
                    _outputManager.Display();
                    Environment.Exit(0);
                    return false;
                default:
                    _outputManager.WriteLine("Invalid selection. Please choose 1 or 2.", ConsoleColor.Red);
                    _outputManager.Display();
                    break;
            }
        }
    }

    // Accept GameEngine so the menu can query world items and add them to the player's inventory
    public void ShowInventoryMenu(IPlayer player, ConsoleRpg.Services.GameEngine engine)
    {
        // Attempt to treat the interface as the concrete Player which contains inventory helpers
        var concrete = player as ConsoleRpgEntities.Models.Characters.Player;
        if (concrete == null)
        {
            _outputManager.WriteLine("Inventory not available for this player.", ConsoleColor.Yellow);
            _outputManager.Display();
            return;
        }

        while (true)
        {
            _outputManager.WriteLine("\nInventory Management:", ConsoleColor.Cyan);
            _outputManager.WriteLine("1. Display inventory", ConsoleColor.Cyan);
            _outputManager.WriteLine("2. Search for item by name", ConsoleColor.Cyan);
            _outputManager.WriteLine("3. List items by type", ConsoleColor.Cyan);
            _outputManager.WriteLine("4. Sort items (submenu)", ConsoleColor.Cyan);
            _outputManager.WriteLine("5. Equip item by name", ConsoleColor.Cyan);
            _outputManager.WriteLine("6. Use item by name", ConsoleColor.Cyan);
            _outputManager.WriteLine("7. Remove item by name/id/partial", ConsoleColor.Cyan);
            _outputManager.WriteLine("8. Browse world items (search/add)", ConsoleColor.Cyan);
            _outputManager.WriteLine("0. Back", ConsoleColor.Cyan);
            _outputManager.Display();

            var input = Console.ReadLine();
            if (input == "0") break;

            switch (input)
            {
                case "1":
                    var items = concrete.Inventory?.Items ?? new List<Item>();
                    if (!items.Any())
                    {
                        _outputManager.WriteLine("Inventory is empty.", ConsoleColor.Yellow);
                        _outputManager.Display();
                        break;
                    }

                    foreach (var it in items.OrderBy(i => i.Name))
                    {
                        _outputManager.WriteLine($"{it.Id}: {it.Name} Type:{it.Type} Atk:{it.Attack} Def:{it.Defense} Wt:{it.Weight} Val:{it.Value}");
                    }
                    _outputManager.Display();
                    break;
                case "2":
                    _outputManager.Write("Search query: ");
                    _outputManager.Display();
                    var q = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(q))
                    {
                        _outputManager.WriteLine("Search cancelled.", ConsoleColor.Yellow);
                        _outputManager.Display();
                        break;
                    }
                    var results = concrete.SearchItems(q).ToList();
                    if (!results.Any())
                    {
                        _outputManager.WriteLine("No items found.", ConsoleColor.Yellow);
                        _outputManager.Display();
                        break;
                    }
                    foreach (var it in results) _outputManager.WriteLine($"{it.Id}: {it.Name}");
                    _outputManager.Display();
                    break;
                case "3":
                    var groups = concrete.ListItemsByType().ToList();
                    if (!groups.Any())
                    {
                        _outputManager.WriteLine("No items to list.", ConsoleColor.Yellow);
                        _outputManager.Display();
                        break;
                    }
                    foreach (var g in groups)
                    {
                        _outputManager.WriteLine($"Type: {g.Key}");
                        foreach (var it in g) _outputManager.WriteLine($"  {it.Id}: {it.Name}");
                    }
                    _outputManager.Display();
                    break;
                case "4":
                    _outputManager.WriteLine("\nSort Options:", ConsoleColor.Cyan);
                    _outputManager.WriteLine("1. Sort by Name", ConsoleColor.Cyan);
                    _outputManager.WriteLine("2. Sort by Attack Value", ConsoleColor.Cyan);
                    _outputManager.WriteLine("3. Sort by Defense Value", ConsoleColor.Cyan);
                    _outputManager.Display();
                    var s = Console.ReadLine();
                    var sorted = (s switch
                    {
                        "1" => concrete.SortByName(),
                        "2" => concrete.SortByAttack(),
                        "3" => concrete.SortByDefense(),
                        _ => concrete.Inventory?.Items ?? Enumerable.Empty<Item>()
                    }).ToList();
                    if (!sorted.Any())
                    {
                        _outputManager.WriteLine("No items to sort.", ConsoleColor.Yellow);
                        _outputManager.Display();
                        break;
                    }
                    foreach (var it in sorted) _outputManager.WriteLine($"{it.Id}: {it.Name} Atk:{it.Attack} Def:{it.Defense}");
                    _outputManager.Display();
                    break;
                case "5":
                    // Show inventory first to help selection
                    var equipItems = concrete.Inventory?.Items ?? new List<Item>();
                    if (!equipItems.Any())
                    {
                        _outputManager.WriteLine("Inventory is empty. Nothing to equip.", ConsoleColor.Yellow);
                        _outputManager.Display();
                        break;
                    }
                    _outputManager.WriteLine("Current inventory:", ConsoleColor.Cyan);
                    foreach (var it in equipItems.OrderBy(i => i.Name)) _outputManager.WriteLine($"{it.Id}: {it.Name} (Atk:{it.Attack} Def:{it.Defense})");
                    _outputManager.Display();

                    _outputManager.Write("Enter item Id, exact name, or partial name to equip (blank to cancel): ");
                    _outputManager.Display();
                    var equipInput = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(equipInput))
                    {
                        _outputManager.WriteLine("Equip cancelled.", ConsoleColor.Yellow);
                        _outputManager.Display();
                        break;
                    }

                    Item? equipSelected = null;
                    if (int.TryParse(equipInput.Trim(), out var eid))
                    {
                        equipSelected = equipItems.FirstOrDefault(i => i.Id == eid);
                        if (equipSelected == null)
                        {
                            _outputManager.WriteLine("No item with that Id in your inventory.", ConsoleColor.Yellow);
                            _outputManager.Display();
                            break;
                        }
                    }
                    else
                    {
                        var matches = concrete.SearchItems(equipInput.Trim()).ToList();
                        if (!matches.Any())
                        {
                            _outputManager.WriteLine("No items match that name or partial.", ConsoleColor.Yellow);
                            _outputManager.Display();
                            break;
                        }
                        if (matches.Count == 1) equipSelected = matches[0];
                        else
                        {
                            _outputManager.WriteLine("Multiple matches found:", ConsoleColor.Cyan);
                            for (var i = 0; i < matches.Count; i++) _outputManager.WriteLine($"{i + 1}. {matches[i].Id}: {matches[i].Name} (Atk:{matches[i].Attack} Def:{matches[i].Defense})");
                            _outputManager.Display();
                            _outputManager.Write("Enter the number of the item to equip (or blank to cancel): ");
                            _outputManager.Display();
                            var pick = Console.ReadLine();
                            if (string.IsNullOrWhiteSpace(pick)) { _outputManager.WriteLine("Equip cancelled.", ConsoleColor.Yellow); _outputManager.Display(); break; }
                            if (!int.TryParse(pick, out var pickIdx) || pickIdx < 1 || pickIdx > matches.Count)
                            {
                                _outputManager.WriteLine("Invalid selection. Equip cancelled.", ConsoleColor.Red);
                                _outputManager.Display();
                                break;
                            }
                            equipSelected = matches[pickIdx - 1];
                        }
                    }

                    // Ensure selection succeeded then try equip and show result
                    if (equipSelected == null)
                    {
                        _outputManager.WriteLine("Equip cancelled.", ConsoleColor.Yellow);
                        _outputManager.Display();
                        break;
                    }
                    var equipResult = concrete.EquipItem(equipSelected.Name);
                    _outputManager.WriteLine(equipResult, equipResult.Contains("equipped") ? ConsoleColor.Green : ConsoleColor.Yellow);
                    _outputManager.Display();
                    break;
                case "6":
                    // Show inventory first to help selection
                    var useItems = concrete.Inventory?.Items ?? new List<Item>();
                    if (!useItems.Any())
                    {
                        _outputManager.WriteLine("Inventory is empty. Nothing to use.", ConsoleColor.Yellow);
                        _outputManager.Display();
                        break;
                    }
                    _outputManager.WriteLine("Current inventory:", ConsoleColor.Cyan);
                    foreach (var it in useItems.OrderBy(i => i.Name)) _outputManager.WriteLine($"{it.Id}: {it.Name} (Type:{it.Type})");
                    _outputManager.Display();

                    _outputManager.Write("Enter item Id, exact name, or partial name to use (blank to cancel): ");
                    _outputManager.Display();
                    var useInput = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(useInput))
                    {
                        _outputManager.WriteLine("Use cancelled.", ConsoleColor.Yellow);
                        _outputManager.Display();
                        break;
                    }

                    Item? useSelected = null;
                    if (int.TryParse(useInput.Trim(), out var uid))
                    {
                        useSelected = useItems.FirstOrDefault(i => i.Id == uid);
                        if (useSelected == null)
                        {
                            _outputManager.WriteLine("No item with that Id in your inventory.", ConsoleColor.Yellow);
                            _outputManager.Display();
                            break;
                        }
                    }
                    else
                    {
                        var matches = concrete.SearchItems(useInput.Trim()).ToList();
                        if (!matches.Any())
                        {
                            _outputManager.WriteLine("No items match that name or partial.", ConsoleColor.Yellow);
                            _outputManager.Display();
                            break;
                        }
                        if (matches.Count == 1) useSelected = matches[0];
                        else
                        {
                            _outputManager.WriteLine("Multiple matches found:", ConsoleColor.Cyan);
                            for (var i = 0; i < matches.Count; i++) _outputManager.WriteLine($"{i + 1}. {matches[i].Id}: {matches[i].Name} (Type:{matches[i].Type})");
                            _outputManager.Display();
                            _outputManager.Write("Enter the number of the item to use (or blank to cancel): ");
                            _outputManager.Display();
                            var pick = Console.ReadLine();
                            if (string.IsNullOrWhiteSpace(pick)) { _outputManager.WriteLine("Use cancelled.", ConsoleColor.Yellow); _outputManager.Display(); break; }
                            if (!int.TryParse(pick, out var pickIdx) || pickIdx < 1 || pickIdx > matches.Count)
                            {
                                _outputManager.WriteLine("Invalid selection. Use cancelled.", ConsoleColor.Red);
                                _outputManager.Display();
                                break;
                            }
                            useSelected = matches[pickIdx - 1];
                        }
                    }

                    // Ensure selection succeeded then try use and show result
                    if (useSelected == null)
                    {
                        _outputManager.WriteLine("Use cancelled.", ConsoleColor.Yellow);
                        _outputManager.Display();
                        break;
                    }
                    var useResult = concrete.UseItem(useSelected.Name);
                    _outputManager.WriteLine(useResult, useResult.Contains("consumed") ? ConsoleColor.Green : ConsoleColor.Yellow);
                    _outputManager.Display();
                    break;
                case "7":
                    // Improved removal: show inventory, accept id/name/partial, disambiguate, confirm
                    var invItems = concrete.Inventory?.Items ?? new List<Item>();
                    if (!invItems.Any())
                    {
                        _outputManager.WriteLine("Inventory is empty. Nothing to remove.", ConsoleColor.Yellow);
                        _outputManager.Display();
                        break;
                    }

                    _outputManager.WriteLine("Current inventory:", ConsoleColor.Cyan);
                    foreach (var it in invItems.OrderBy(i => i.Name))
                    {
                        _outputManager.WriteLine($"{it.Id}: {it.Name} (Type:{it.Type} Atk:{it.Attack} Def:{it.Defense})");
                    }
                    _outputManager.Display();

                    _outputManager.Write("Enter item Id, exact name, or partial name to remove (blank to cancel): ");
                    _outputManager.Display();
                    var removeInput = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(removeInput))
                    {
                        _outputManager.WriteLine("Remove cancelled.", ConsoleColor.Yellow);
                        _outputManager.Display();
                        break;
                    }

                    Item selected = null;

                    // Try parse as Id first
                    if (int.TryParse(removeInput.Trim(), out var rid))
                    {
                        selected = invItems.FirstOrDefault(i => i.Id == rid);
                        if (selected == null)
                        {
                            _outputManager.WriteLine("No item with that Id in your inventory.", ConsoleColor.Yellow);
                            _outputManager.Display();
                            break;
                        }
                    }
                    else
                    {
                        // Use SearchItems to allow partial matches
                        var matches = concrete.SearchItems(removeInput.Trim()).ToList();
                        if (!matches.Any())
                        {
                            _outputManager.WriteLine("No items match that name or partial.", ConsoleColor.Yellow);
                            _outputManager.Display();
                            break;
                        }
                        if (matches.Count == 1)
                        {
                            selected = matches[0];
                        }
                        else
                        {
                            _outputManager.WriteLine("Multiple matches found:", ConsoleColor.Cyan);
                            for (var i = 0; i < matches.Count; i++)
                            {
                                var m = matches[i];
                                _outputManager.WriteLine($"{i + 1}. {m.Id}: {m.Name} (Atk:{m.Attack} Def:{m.Defense})");
                            }
                            _outputManager.Display();
                            _outputManager.Write("Enter the number of the item to remove (or blank to cancel): ");
                            _outputManager.Display();
                            var pick = Console.ReadLine();
                            if (string.IsNullOrWhiteSpace(pick))
                            {
                                _outputManager.WriteLine("Remove cancelled.", ConsoleColor.Yellow);
                                _outputManager.Display();
                                break;
                            }
                            if (!int.TryParse(pick, out var pickIdx) || pickIdx < 1 || pickIdx > matches.Count)
                            {
                                _outputManager.WriteLine("Invalid selection. Remove cancelled.", ConsoleColor.Red);
                                _outputManager.Display();
                                break;
                            }
                            selected = matches[pickIdx - 1];
                        }
                    }

                    // Confirm
                    _outputManager.Write($"Confirm removal of '{selected.Name}'? (y/N): ");
                    _outputManager.Display();
                    var confirm = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(confirm) || !confirm.Trim().Equals("y", StringComparison.OrdinalIgnoreCase))
                    {
                        _outputManager.WriteLine("Remove cancelled.", ConsoleColor.Yellow);
                        _outputManager.Display();
                        break;
                    }

                    var ok = concrete.RemoveItem(selected.Name);
                    if (ok)
                    {
                        _outputManager.WriteLine($"Removed '{selected.Name}'.", ConsoleColor.Green);
                        // Display updated inventory
                        var remaining = concrete.Inventory?.Items ?? new List<Item>();
                        if (!remaining.Any())
                        {
                            _outputManager.WriteLine("Inventory is now empty.", ConsoleColor.Yellow);
                        }
                        else
                        {
                            _outputManager.WriteLine("Updated inventory:", ConsoleColor.Cyan);
                            foreach (var it in remaining.OrderBy(i => i.Name))
                            {
                                _outputManager.WriteLine($"{it.Id}: {it.Name} (Type:{it.Type} Atk:{it.Attack} Def:{it.Defense})");
                            }
                        }
                    }
                    else
                    {
                        _outputManager.WriteLine("Failed to remove item.", ConsoleColor.Red);
                    }
                    _outputManager.Display();
                    break;
                case "8":
                    // Browse world items and allow adding to player's inventory
                    if (engine == null)
                    {
                        _outputManager.WriteLine("Game engine not available.", ConsoleColor.Yellow);
                        _outputManager.Display();
                        break;
                    }

                    _outputManager.Write("Search world items (leave empty for sample): ");
                    _outputManager.Display();
                    var query = Console.ReadLine();
                    var worldItems = engine.FindWorldItems(query ?? string.Empty).ToList();
                    if (!worldItems.Any())
                    {
                        _outputManager.WriteLine("No world items found.", ConsoleColor.Yellow);
                        _outputManager.Display();
                        break;
                    }

                    foreach (var wi in worldItems)
                    {
                        _outputManager.WriteLine($"{wi.Id}: {wi.Name} Type:{wi.Type} Atk:{wi.Attack} Def:{wi.Defense} Wt:{wi.Weight} Val:{wi.Value}");
                    }
                    _outputManager.Display();

                    _outputManager.Write("Enter item Id to add to your inventory (or blank to cancel): ");
                    _outputManager.Display();
                    var idInput = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(idInput))
                    {
                        _outputManager.WriteLine("Add cancelled.", ConsoleColor.Yellow);
                        _outputManager.Display();
                        break;
                    }
                    if (!int.TryParse(idInput, out var itemId))
                    {
                        _outputManager.WriteLine("Invalid Id.", ConsoleColor.Red);
                        _outputManager.Display();
                        break;
                    }

                    if (engine.TryAddItemToPlayerById(itemId, out var msg))
                    {
                        _outputManager.WriteLine(msg, ConsoleColor.Green);
                    }
                    else
                    {
                        _outputManager.WriteLine(msg, ConsoleColor.Yellow);
                    }
                    _outputManager.Display();
                    break;
                default:
                    _outputManager.WriteLine("Invalid selection.", ConsoleColor.Red);
                    _outputManager.Display();
                    break;
            }
        }
    }
}
