using ConsoleRpgEntities.Models.Equipments;

namespace ConsoleRpgEntities.Services
{
    /// <summary>
    /// Manages equipment operations including equipping and unequipping items.
    /// </summary>
    public class EquipmentManager
    {
        private Equipment _equipment;

        public EquipmentManager(Equipment? equipment = null)
        {
            _equipment = equipment ?? new Equipment();
        }

        public Equipment Equipment => _equipment;

        /// <summary>
        /// Equips an item as either a weapon or armor based on its type and stats.
        /// </summary>
        public string EquipItem(Item item)
        {
            if (item == null) return "Item is null.";
            if (item.Attack == 0 && item.Defense == 0) return "Item has no equip stats.";

            var type = (item.Type ?? string.Empty).Trim().ToLowerInvariant();
            
            // Determine slot based on type or stats
            if (type == "weapon" || item.Attack > item.Defense)
            {
                _equipment.Weapon = item;
                _equipment.WeaponId = item.Id;
                return $"{item.Name} equipped as weapon.";
            }
            else if (type == "armor" || item.Defense >= item.Attack)
            {
                _equipment.Armor = item;
                _equipment.ArmorId = item.Id;
                return $"{item.Name} equipped as armor.";
            }
            else
            {
                return "Unable to determine equip slot for item.";
            }
        }

        /// <summary>
        /// Unequips an item if it matches the currently equipped weapon or armor.
        /// </summary>
        public void UnequipItem(Item item)
        {
            if (item == null) return;

            if (_equipment.WeaponId == item.Id)
            {
                _equipment.Weapon = null;
                _equipment.WeaponId = null;
            }
            
            if (_equipment.ArmorId == item.Id)
            {
                _equipment.Armor = null;
                _equipment.ArmorId = null;
            }
        }

        /// <summary>
        /// Gets the total attack bonus from equipped items.
        /// </summary>
        public int GetTotalAttack()
        {
            return (_equipment.Weapon?.Attack ?? 0);
        }

        /// <summary>
        /// Gets the total defense bonus from equipped items.
        /// </summary>
        public int GetTotalDefense()
        {
            return (_equipment.Armor?.Defense ?? 0);
        }
    }
}

