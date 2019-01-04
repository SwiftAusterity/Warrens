using NetMud.DataStructure.Gaia;

namespace NetMud.DataStructure.Architectural
{
    /// <summary>
    /// Indicates that this content type belongs to a world set of content
    /// </summary>
    public interface IBelongToAWorld
    {
        /// <summary>
        /// The world this belongs to
        /// </summary>
        IGaiaTemplate OwnerWorld { get; set; }
    }
}
