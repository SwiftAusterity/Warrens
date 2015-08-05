using NetMud.DataStructure.Base.Supporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.Data.Reference
{
    /// <summary>
    /// A single 11x11 grid of the 11 planes that compose the dimensional model
    /// </summary>
    [Serializable]
    public class DimensionalModelPlane : IDimensionalModelPlane
    {
        /// <summary>
        /// New up an empty model plane
        /// </summary>
        public DimensionalModelPlane()
        {
            ModelNodes = new HashSet<IDimensionalModelNode>();
        }

        /// <summary>
        /// The collection of 121 nodes in the plane
        /// </summary>
        public HashSet<IDimensionalModelNode> ModelNodes { get; set; }

        /// <summary>
        /// The name of this plane (for a sword it might be 'Blade' or 'Hilt')
        /// </summary>
        public string TagName { get; set; }

        /// <summary>
        /// All nodes in a plane are of the same YAxis so bubble it up here so we have access
        /// </summary>
        public short YAxis { get; set; }

        /// <summary>
        /// Gets a node based on the X and Y axis
        /// </summary>
        /// <param name="xAxis">the X-Axis of the node to get</param>
        /// <returns>the node</returns>
        public IDimensionalModelNode GetNode(short xAxis, short zAxis)
        {
            return ModelNodes.FirstOrDefault(node => node.XAxis.Equals(xAxis) && node.ZAxis.Equals(zAxis));
        }
    }
}
