using NetMud.DataStructure.Base.Supporting;
using System;

namespace NetMud.Data.Reference
{
    [Serializable]
    public class DimensionalModelNode : IDimensionalModelNode
    {
        /// <summary>
        /// The position of this node on the XAxis
        /// </summary>
        public short XAxis { get; set; }

        /// <summary>
        /// The position of this node on the ZAxis
        /// </summary>
        public short ZAxis { get; set; }

        /// <summary>
        /// The Y-axis from the plane this belongs to
        /// </summary>
        public short YAxis { get; set; }

        /// <summary>
        /// The damage type inflicted when this part of the model strikes
        /// </summary>
        public DamageType Style { get; set; }

        /// <summary>
        /// Material composition of the node
        /// </summary>
        public IMaterial Composition { get; set; }
    }
}
