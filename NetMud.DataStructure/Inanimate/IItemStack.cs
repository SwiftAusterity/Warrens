

namespace NetMud.DataStructure.Inanimate
{
    /// <summary>
    /// A stack of Items
    /// </summary>
    public interface IItemStack
    {
        /// <summary>
        /// The item template
        /// </summary>
        IInanimateTemplate Item { get; set; }

        /// <summary>
        /// The size of the entire stack
        /// </summary>
        int FullStackSize { get; set; }

        /// <summary>
        /// The rendered description (including quality breakdowns)
        /// </summary>
        string Description { get; set; }
    }
}
