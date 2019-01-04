using NetMud.DataStructure.Action;
using System.Collections.Generic;

namespace NetMud.DataStructure.Inanimate
{
    /// <summary>
    /// Backing data for "object"s
    /// </summary>
    public interface IInanimateFramework : ICanAccumulate
    {
        /// <summary>
        /// Character->tile interactions
        /// </summary>
        HashSet<IUse> Uses { get; set; }
    }
}
