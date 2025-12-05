using ConsoleRpgEntities.Models.Equipments;

namespace ConsoleRpgEntities.Services
{
    /// <summary>
    /// Manages consumable item usage (e.g., potions).
    /// </summary>
    public class ItemUsageService
    {
        /// <summary>
        /// Uses a consumable item and applies its effects.
        /// Returns a tuple with (success, message).
        /// </summary>
        public (bool Success, string Message) UseItem(Item item, InventoryManager inventoryManager)
        {
            if (item == null) 
                return (false, "Item is null.");

            var type = (item.Type ?? string.Empty).Trim().ToLowerInvariant();
            
            if (type == "potion")
            {
                // Remove the item from inventory
                if (inventoryManager.RemoveItem(item.Name))
                {
                    // Here you could apply effects based on potion name
                    // For example: if potion name contains "healing", restore health
                    return (true, $"{item.Name} consumed.");
                }
                else
                {
                    return (false, "Failed to remove item from inventory.");
                }
            }

            return (false, $"{item.Name} cannot be consumed.");
        }

        /// <summary>
        /// Determines if an item is consumable.
        /// </summary>
        public bool IsConsumable(Item item)
        {
            if (item == null) return false;
            
            var type = (item.Type ?? string.Empty).Trim().ToLowerInvariant();
            return type == "potion";
        }
    }
}

