// I probably should have split this file into multiple smaller files regarding inventory management
// The code works fine as is, but for better maintainability, consider refactoring in the future.
// If you have any suggestions or improvements, feel free to share!

using ConsoleRpgEntities.Models.Abilities.PlayerAbilities;
using ConsoleRpgEntities.Models.Attributes;
using ConsoleRpgEntities.Models.Equipments;

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

        // --- Weight helpers ---
        public decimal GetTotalWeight() =>
            Inventory?.Items?.Sum(i => Convert.ToDecimal(i?.Weight ?? 0m)) ?? 0m;

        public bool CanAddItem(Item item)
        {
            if (item == null) return false;
            var itemWeight = Convert.ToDecimal(item.Weight);
            if (itemWeight < 0) return false;
            return (GetTotalWeight() + itemWeight) <= MaxWeight;
        }

        // --- Inventory operations ---
        public bool AddItem(Item? item)
        {
            if (item == null) return false;

            Inventory ??= new Inventory { PlayerId = Id, Player = this };
            Inventory.Items ??= new List<Item>();

            if (!CanAddItem(item)) return false;

            Inventory.Items.Add(item);
            return true;
        }

        public bool RemoveItem(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            if (Inventory?.Items == null) return false;

            var item = Inventory.Items.FirstOrDefault(i =>
                !string.IsNullOrEmpty(i?.Name) &&
                i.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));

            if (item == null) return false;

            Inventory.Items.Remove(item);

            if (Equipment != null)
            {
                if (Equipment.WeaponId == item.Id) { Equipment.Weapon = null; Equipment.WeaponId = null; }
                if (Equipment.ArmorId == item.Id) { Equipment.Armor = null; Equipment.ArmorId = null; }
            }
            return true;
        }

        public IEnumerable<Item> SearchItems(string namePart)
        {
            if (Inventory?.Items == null) return Enumerable.Empty<Item>();
            if (string.IsNullOrWhiteSpace(namePart)) return Inventory.Items;
            return Inventory.Items
                .Where(i => i.Name != null &&
                            i.Name.IndexOf(namePart, System.StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
        }

        public IEnumerable<IGrouping<string, Item>> ListItemsByType()
        {
            if (Inventory?.Items == null) return Enumerable.Empty<IGrouping<string, Item>>();
            return Inventory.Items.GroupBy(i => i.Type).ToList();
        }

        public IEnumerable<Item> SortByName()
        {
            if (Inventory?.Items == null) return Enumerable.Empty<Item>();
            return Inventory.Items.OrderBy(i => i.Name).ToList();
        }

        public IEnumerable<Item> SortByAttack()
        {
            if (Inventory?.Items == null) return Enumerable.Empty<Item>();
            return Inventory.Items.OrderByDescending(i => i.Attack).ToList();
        }

        public IEnumerable<Item> SortByDefense()
        {
            if (Inventory?.Items == null) return Enumerable.Empty<Item>();
            return Inventory.Items.OrderByDescending(i => i.Defense).ToList();
        }

        // Equip logic: set Equipment.Weapon or Equipment.Armor based on item.Type (e.g., "Weapon" / "Armor")
        public string EquipItem(string name)
        {
            if (Inventory?.Items == null) return "No inventory.";
            var item = Inventory.Items.FirstOrDefault(i => i.Name != null &&
                i.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (item == null) return "Item not found.";
            if (item.Attack == 0 && item.Defense == 0) return "Item has no equip stats.";

            Equipment ??= new Equipment();

            var type = (item.Type ?? string.Empty).Trim().ToLowerInvariant();
            if (type == "weapon" || item.Attack > item.Defense)
            {
                Equipment.Weapon = item;
                Equipment.WeaponId = item.Id;
                return $"{item.Name} equipped as weapon.";
            }
            else if (type == "armor" || item.Defense >= item.Attack)
            {
                Equipment.Armor = item;
                Equipment.ArmorId = item.Id;
                return $"{item.Name} equipped as armor.";
            }
            else
            {
                return "Unable to determine equip slot for item.";
            }
        }

        // Use logic: simple example for potions (remove on use)
        public string UseItem(string name)
        {
            if (Inventory?.Items == null) return "No inventory.";
            var item = Inventory.Items.FirstOrDefault(i => i.Name != null &&
                i.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (item == null) return "Item not found.";

            var type = (item.Type ?? string.Empty).Trim().ToLowerInvariant();
            if (type == "potion")
            {
                Inventory.Items.Remove(item);
                // apply effects here (example: restore health)
                return $"{item.Name} consumed.";
            }

            return $"{item.Name} cannot be consumed.";
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