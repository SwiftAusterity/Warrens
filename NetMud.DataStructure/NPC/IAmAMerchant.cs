using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Inanimate;

namespace NetMud.DataStructure.NPC
{
    /// <summary>
    /// Qualifies NPCs to be merchants, allows for merchant commands
    /// </summary>
    public interface IAmAMerchant : IHaveInventoryToSell
    {
        /// <summary>
        /// Indicates whether this is actually a merchant or not who sells things
        /// </summary>
        /// <returns>if they sell things (but not if they have stock)</returns>
        bool DoISellThings();

        /// <summary>
        /// Indicates whether this is actually a merchant who buys things
        /// </summary>
        /// <returns>if they buy things (but not if they have the money to do so)</returns>
        bool DoIBuyThings();

        /// <summary>
        /// Check the price this will be sold for
        /// </summary>
        /// <param name="item">The item in question</param>
        /// <returns>the price, -1 indicates it wont be sold or isn't in stock</returns>
        int PriceCheck(IInanimate item, bool mustBeInStock);

        /// <summary>
        /// What will the merchant buy this item for
        /// </summary>
        /// <param name="item">the item in question</param>
        /// <returns>the price, -1 indicates they wont buy it</returns>
        int HaggleCheck(IInanimate item);

        /// <summary>
        /// Render the inventory to someone
        /// </summary>
        /// <param name="customer">The person asking</param>
        /// <returns>the inventory display</returns>
        string RenderInventory(IEntity customer);

        /// <summary>
        /// Render the buy list to someone
        /// </summary>
        /// <param name="customer">The person asking</param>
        /// <returns>the biuy list display</returns>
        string RenderPurchaseSheet(IEntity customer);

        /// <summary>
        /// Execute a sale
        /// </summary>
        /// <param name="item">the item in question</param>
        /// <param name="price">the sale price</param>
        /// <returns>error or blank</returns>
        string MakeSale(IMobile customer, IInanimate item, int price);

        /// <summary>
        /// Execute a purchase
        /// </summary>
        /// <param name="item">the item in question</param>
        /// <param name="price">the sale price</param>
        /// <returns>error or blank</returns>
        string MakePurchase(IMobile customer, IInanimate item, int price);
    }

}
