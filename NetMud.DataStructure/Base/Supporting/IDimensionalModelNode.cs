namespace NetMud.DataStructure.Base.Supporting
{
    /// <summary>
    /// A single node of the 121 nodes per plane in the dimensional model
    /// </summary>
    public interface IDimensionalModelNode
    {
        /// <summary>
        /// The position of this node on the XAxis
        /// </summary>
        short XAxis { get; set; }

        /// <summary>
        /// The position of this node on the YAxis
        /// </summary>
        short ZAxis { get; set; }

        /// <summary>
        /// All nodes in a plane are of the same YAxis so bubble it up here so we have access
        /// </summary>
        short YAxis { get; set; }

        /// <summary>
        /// The damage type inflicted when this part of the model strikes
        /// </summary>
        DamageType Style { get; set; }

        /// <summary>
        /// Material composition of the node
        /// </summary>
        IMaterial Composition { get; set; }
    }
}
