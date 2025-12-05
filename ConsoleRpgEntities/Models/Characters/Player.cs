// I probably should have split this file into multiple smaller files regarding inventory management
// The code works fine as is, but for better maintainability, consider refactoring in the future.
// If you have any suggestions or improvements, feel free to share!

using ConsoleRpgEntities.Models.Abilities.PlayerAbilities;
using ConsoleRpgEntities.Models.Attributes;
using ConsoleRpgEntities.Models.Equipments;
using ConsoleRpgEntities.Services;

namespace ConsoleRpgEntities.Models.Characters
{
    public class Player : ITargetable, IPlayer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Experience { get; set; }
        public int Health { get; set; }

        // Foreign key
        public int? EquipmentId { get; set; }

        // Navigation properties
        public virtual Inventory Inventory { get; set; }
        public virtual Equipment Equipment { get; set; }
        public virtual ICollection<Ability> Abilities { get; set; }

        // Stretch: weight limit
        public decimal MaxWeight { get; set; } = 100m;

        // Service managers - lazy initialization to work with EF Core
        private InventoryManager? _inventoryManager;
        private EquipmentManager? _equipmentManager;
        private ItemUsageService? _itemUsageService;

        /// <summary>
        /// Gets the inventory manager for this player.
        /// </summary>
        public InventoryManager GetInventoryManager()
        {
            if (_inventoryManager == null)
            {
                Inventory ??= new Inventory { PlayerId = Id, Player = this };
                _inventoryManager = new InventoryManager(Inventory, MaxWeight);
            }
            return _inventoryManager;
        }

        /// <summary>
        /// Gets the equipment manager for this player.
        /// </summary>
        public EquipmentManager GetEquipmentManager()
        {
            if (_equipmentManager == null)
            {
                Equipment ??= new Equipment();
                _equipmentManager = new EquipmentManager(Equipment);
            }
            return _equipmentManager;
        }

        /// <summary>
        /// Gets the item usage service for this player.
        /// </summary>
        public ItemUsageService GetItemUsageService()
        {
            _itemUsageService ??= new ItemUsageService();
            return _itemUsageService;
        }

        // --- Delegate methods to service managers ---
        // These methods provide backward compatibility and delegate to the appropriate service

        public decimal GetTotalWeight() => GetInventoryManager().GetTotalWeight();

        public bool CanAddItem(Item item) => GetInventoryManager().CanAddItem(item);

        // --- Inventory operations ---
        public bool AddItem(Item? item) => GetInventoryManager().AddItem(item);

        public bool RemoveItem(string name)
        {
            var item = GetInventoryManager().FindItemByName(name);
            if (item == null) return false;

            // Unequip if currently equipped
            GetEquipmentManager().UnequipItem(item);

            // Remove from inventory
            return GetInventoryManager().RemoveItem(name);
        }

        public IEnumerable<Item> SearchItems(string namePart) => 
            GetInventoryManager().SearchItems(namePart);

        public IEnumerable<IGrouping<string, Item>> ListItemsByType() => 
            GetInventoryManager().ListItemsByType();

        public IEnumerable<Item> SortByName() => 
            GetInventoryManager().SortByName();

        public IEnumerable<Item> SortByAttack() => 
            GetInventoryManager().SortByAttack();

        public IEnumerable<Item> SortByDefense() => 
            GetInventoryManager().SortByDefense();

        // Equip logic: set Equipment.Weapon or Equipment.Armor based on item.Type (e.g., "Weapon" / "Armor")
        public string EquipItem(string name)
        {
            var item = GetInventoryManager().FindItemByName(name);
            if (item == null) return "Item not found.";
            
            return GetEquipmentManager().EquipItem(item);
        }

        // Use logic: simple example for potions (remove on use)
        public string UseItem(string name)
        {
            var item = GetInventoryManager().FindItemByName(name);
            if (item == null) return "Item not found.";

            var (success, message) = GetItemUsageService().UseItem(item, GetInventoryManager());
            return message;
        }

        public void Attack(ITargetable target)
        {
            // Player-specific attack logic
            var weaponName = Equipment?.Weapon?.Name ?? "fists";
            var weaponAttack = Equipment?.Weapon?.Attack ?? 1;
            Console.WriteLine($"{Name} attacks {target.Name} with {weaponName} dealing {weaponAttack} damage!");
            target.Health -= weaponAttack;
            Console.WriteLine($"{target.Name} has {target.Health} health remaining.");
        }

        public void UseAbility(IAbility ability, ITargetable target)
        {
            if (Abilities != null && Abilities.Contains(ability))
            {
                ability.Activate(this, target);
            }
            else
            {
                Console.WriteLine($"{Name} does not have the ability {ability.Name}!");
            }
        }
    }
}