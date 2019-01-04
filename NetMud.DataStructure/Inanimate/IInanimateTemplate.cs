using NetMud.DataStructure.Architectural.EntityBase;
using System.Collections.Generic;

namespace NetMud.DataStructure.Inanimate
{
    /// <summary>
    /// Backing data for "object"s
    /// </summary>
    public interface IInanimateTemplate : ITemplate, IInanimateFramework, ICanBeCrafted
    {
        /// <summary>
        /// Is this item part of the randomized debris set for its gaia?
        /// </summary>
        bool RandomDebris { get; set; }

        /// <summary>
        /// Definition for the room's capacity for inanimates
        /// </summary>
        HashSet<IEntityContainerData<IInanimate>> InanimateContainers { get; set; }
    }
}
