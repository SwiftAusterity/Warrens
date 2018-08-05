using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.SupportingClasses;
using System.Collections.Generic;

namespace NetMud.DataStructure.Base.EntityBackingData
{
    /// <summary>
    /// Backing data for "object"s
    /// </summary>
    public interface IInanimateData : IEntityBackingData, IDescribable
    {
        /// <summary>
        /// Definition for the room's capacity for mobiles
        /// </summary>
        HashSet<IEntityContainerData<IMobile>> MobileContainers { get; set; }

        /// <summary>
        /// Definition for the room's capacity for inanimates
        /// </summary>
        HashSet<IEntityContainerData<IInanimate>> InanimateContainers { get; set; }


        IDictionary<IInanimateData, short> InternalComposition { get; set; }

        /// <summary>
        /// The current physical model for this entity
        /// </summary>
        IDimensionalModel Model { get; }
    }
}
