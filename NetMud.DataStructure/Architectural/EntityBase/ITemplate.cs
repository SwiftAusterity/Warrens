using NetMud.DataStructure.Action;
using System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Architectural.EntityBase
{
    /// <summary>
    /// Mid-point Interface for entity backing data
    /// </summary>
    public interface ITemplate : IKeyedData, IVisible, IHaveQualities, IBelongToAWorld
    {
        /// <summary>
        /// Entity class this backing data attaches to
        /// </summary>
        Type EntityClass { get; }

        /// <summary>
        /// Keywords this entity can be found with in command parsing (needed for admin commands that look for data)
        /// </summary>
        string[] Keywords { get; set; }

        /// <summary>
        /// Character->tile interactions
        /// </summary>
        HashSet<IInteraction> Interactions { get; set; }

        /// <summary>
        /// spawn interactions to pass onto entities as they are popped
        /// </summary>
        HashSet<IDecayEvent> DecayEvents { get; set; }
    }
}
