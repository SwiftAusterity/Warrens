using NetMud.DataStructure.Base.System;

namespace NetMud.DataStructure.Behaviors.Existential
{
    public interface IAccumulation : IEntity
    {
        /// <summary>
        /// How many of the thing is there
        /// </summary>
        int AccumulationAmount { get; set; }

        /// <summary>
        /// How much mass does it occupy as an aggregate
        /// </summary>
        int TotalVolume { get; }

        /// <summary>
        /// How much does it weigh as an aggregate
        /// </summary>
        int TotalWeight { get; }
    }
}
