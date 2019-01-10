using System.Collections.Generic;

namespace NetMud.DataStructure.NPC
{
    /// <summary>
    /// Ensures NPCs have stock for being merchants
    /// </summary>
    public interface IHaveInventoryToSell
    {
        /// <summary>
        /// What this merchant is willing to purchase
        /// </summary>
        HashSet<IMerchandise> WillPurchase { get; set; }

        /// <summary>
        /// What this merchant is willing to sell
        /// </summary>
        HashSet<IMerchandise> WillSell { get; set; }

        /// <summary>
        /// Inventory this merchant will generate on a timer Item, Quantity
        /// </summary>
        HashSet<MerchandiseStock> InventoryRestock { get; set; }
    }
}
