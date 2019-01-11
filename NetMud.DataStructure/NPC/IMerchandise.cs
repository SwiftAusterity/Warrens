using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Inanimate;

namespace NetMud.DataStructure.NPC
{
    /// <summary>
    /// Criteria for buying and selling merchandise
    /// </summary>
    public interface IMerchandise
    {
        /// <summary>
        /// Item type
        /// </summary>
        IInanimateTemplate Item { get; set; }

        /// <summary>
        /// Required quality
        /// </summary>
        string Quality { get; set; }

        /// <summary>
        /// Range for the quality
        /// </summary>
        ValueRange<int> QualityRange { get; set; }

        /// <summary>
        /// Markup or discount for buying/selling. 1 would be no markup/discount, below 1 would be discount
        /// </summary>
        decimal MarkRate { get; set; }
    }
}
