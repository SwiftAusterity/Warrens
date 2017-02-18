using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Behaviors.System;

namespace NetMud.DataStructure.Base.Supporting
{
    /// <summary>
    /// Plants, all elements can be nullable (one has to exist)
    /// </summary>
    public interface IFlora : INaturalResource
    {
        /// <summary>
        /// How much sunlight does this need to spawn
        /// </summary>
        int SunlightPreference { get; set; }

        /// <summary>
        /// Does this plant go dormant in colder weather
        /// </summary>
        bool Coniferous { get; set; }

        /// <summary>
        /// Bulk material of plant. Stem, trunk, etc.
        /// </summary>
        IMaterial Wood { get; set; }

        /// <summary>
        /// Flowering element of plant
        /// </summary>
        IInanimateData Flower { get; set; }

        /// <summary>
        /// Leaves of the plant.
        /// </summary>
        IInanimateData Leaf { get; set; }

        /// <summary>
        /// Fruit of the plant, can be inedible like a pinecone
        /// </summary>
        IInanimateData Fruit { get; set; }

        /// <summary>
        /// Seed of the plant.
        /// </summary>
        IInanimateData Seed { get; set; }
    }
}
