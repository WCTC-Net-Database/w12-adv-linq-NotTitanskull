using ConsoleRpgEntities.Models.Equipments;

namespace ConsoleRpgEntities.Services
{
    /// <summary>
    /// Manages inventory operations for a player including adding, removing, searching, and sorting items.
    /// </summary>
    public class InventoryManager
    {
        private readonly Inventory _inventory;
        private readonly decimal _maxWeight;

        public InventoryManager(Inventory inventory, decimal maxWeight = 100m)
        {
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            _maxWeight = maxWeight;
        }

        /// <summary>
        /// Calculates the total weight of all items in the inventory.
        /// </summary>
        public decimal GetTotalWeight()
        {
            return _inventory?.Items?.Sum(i => Convert.ToDecimal(i?.Weight ?? 0m)) ?? 0m;
        }

        /// <summary>
        /// Checks if an item can be added to the inventory based on weight constraints.
        /// </summary>
        public bool CanAddItem(Item item)
        {
            if (item == null) return false;
            var itemWeight = Convert.ToDecimal(item.Weight);
            if (itemWeight < 0) return false;
            return (GetTotalWeight() + itemWeight) <= _maxWeight;
        }

        /// <summary>
        /// Adds an item to the inventory if weight allows.
        /// </summary>
        public bool AddItem(Item? item)
        {
            if (item == null) return false;

            _inventory.Items ??= new List<Item>();

            if (!CanAddItem(item)) return false;

            _inventory.Items.Add(item);
            return true;
        }

        /// <summary>
        /// Removes an item from the inventory by name (case-insensitive).
        /// </summary>
        public bool RemoveItem(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            if (_inventory?.Items == null) return false;

            var item = _inventory.Items.FirstOrDefault(i =>
                !string.IsNullOrEmpty(i?.Name) &&
                i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (item == null) return false;

            _inventory.Items.Remove(item);
            return true;
        }

        /// <summary>
        /// Searches for items by partial name match (case-insensitive).
        /// </summary>
        public IEnumerable<Item> SearchItems(string namePart)
        {
            if (_inventory?.Items == null) return Enumerable.Empty<Item>();
            if (string.IsNullOrWhiteSpace(namePart)) return _inventory.Items;
            
            return _inventory.Items
                .Where(i => i.Name != null &&
                            i.Name.IndexOf(namePart, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
        }

        /// <summary>
        /// Groups items by their type.
        /// </summary>
        public IEnumerable<IGrouping<string, Item>> ListItemsByType()
        {
            if (_inventory?.Items == null) return Enumerable.Empty<IGrouping<string, Item>>();
            return _inventory.Items.GroupBy(i => i.Type).ToList();
        }

        /// <summary>
        /// Sorts items alphabetically by name.
        /// </summary>
        public IEnumerable<Item> SortByName()
        {
            if (_inventory?.Items == null) return Enumerable.Empty<Item>();
            return _inventory.Items.OrderBy(i => i.Name).ToList();
        }

        /// <summary>
        /// Sorts items by attack value (descending).
        /// </summary>
        public IEnumerable<Item> SortByAttack()
        {
            if (_inventory?.Items == null) return Enumerable.Empty<Item>();
            return _inventory.Items.OrderByDescending(i => i.Attack).ToList();
        }

        /// <summary>
        /// Sorts items by defense value (descending).
        /// </summary>
        public IEnumerable<Item> SortByDefense()
        {
            if (_inventory?.Items == null) return Enumerable.Empty<Item>();
            return _inventory.Items.OrderByDescending(i => i.Defense).ToList();
        }

        /// <summary>
        /// Gets all items in the inventory.
        /// </summary>
        public IEnumerable<Item> GetAllItems()
        {
            return _inventory?.Items ?? Enumerable.Empty<Item>();
        }

        /// <summary>
        /// Finds an item by name (case-insensitive).
        /// </summary>
        public Item? FindItemByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            if (_inventory?.Items == null) return null;

            return _inventory.Items.FirstOrDefault(i => 
                i.Name != null && 
                i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }
}

