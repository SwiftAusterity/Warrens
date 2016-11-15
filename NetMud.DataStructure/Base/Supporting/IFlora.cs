using NetMud.DataStructure.Behaviors.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.DataStructure.Base.Supporting
{
    /// <summary>
    /// Plants, all elements can be nullable (one has to exist)
    /// </summary>
    public interface IFlora : INaturalResource
    {
        /// <summary>
        /// Bulk material of plant. Stem, trunk, etc.
        /// </summary>
        IMaterial Wood { get; set; }

        /// <summary>
        /// Flowering element of plant
        /// </summary>
        IMaterial Flower { get; set; }

        /// <summary>
        /// Leaves of the plant.
        /// </summary>
        IMaterial Leaf { get; set; }

        /// <summary>
        /// Fruit of the plant, can be inedible like a pinecone
        /// </summary>
        IMaterial Fruit { get; set; }

        /// <summary>
        /// Seed of the plant.
        /// </summary>
        IMaterial Seed { get; set; }
    }
}
