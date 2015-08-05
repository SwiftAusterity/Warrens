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
        /// The collection of 121 nodes in the plane
        /// </summary>
        public HashSet<IDimensionalModelNode> ModelNodes { get; set; }

        /// <summary>
        /// The name of this plane (for a sword it might be 'Blade' or 'Hilt')
        /// </summary>
        public string TagName { get; set; }
    }
}
