using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.SupportingClasses;

namespace NetMud.DataStructure.Base.Supporting
{
    /// <summary>
    /// Pathway/Exit for going from and to larger structures
    /// </summary>
    public interface IHorizon<T> where T : ILocation
    {
        /// <summary>
        /// Location this pathway leads to
        /// </summary>
        T ToLocation { get; set; }

        /// <summary>
        /// The visual output of using this path
        /// </summary>
        IMessageCluster Output { get; set; }
    }
}
