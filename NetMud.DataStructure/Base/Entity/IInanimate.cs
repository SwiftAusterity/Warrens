using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Behaviors.Existential;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.Behaviors.System;

namespace NetMud.DataStructure.Base.Entity
{
    /// <summary>
    /// "Object" entity
    /// </summary>
    public interface IInanimate : IActor, ILocation, ICanBeWorn, ICanBeHeld, ISpawnAsMultiple
    {
        /// <summary>
        /// The current physical model for this entity
        /// </summary>
        IDimensionalModel Model { get; }
    }
}
