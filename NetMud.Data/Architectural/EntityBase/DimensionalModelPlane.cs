using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataStructure.Architectural.EntityBase;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace NetMud.Data.Architectural.EntityBase
{
    /// <summary>
    /// A single 11x11 grid of the 21 planes that compose the dimensional model
    /// </summary>
    [Serializable]
    public class DimensionalModelPlane : IDimensionalModelPlane
    {
        /// <summary>
        /// The collection of [math] nodes in the plane
        /// </summary>
        [UIHint("DimensionalModelNodes")]
        [DimensionalModelNodeDataBinder]
        public HashSet<IDimensionalModelNode> ModelNodes { get; set; }

        /// <summary>
        /// The name of this plane (for a sword it might be 'Blade' or 'Hilt')
        /// </summary>
        [Display(Name = "Name", Description = "Descriptive name for this Y-axis row. Things like Blade, Hilt, Handle, etc.")]
        [DataType(DataType.Text)]
        public string TagName { get; set; }

        /// <summary>
        /// All nodes in a plane are of the same YAxis so bubble it up here so we have access
        /// </summary>
        [Display(Name = "Y-Axis", Description = "The Y-Axis of the model this row is for.")]
        [DataType(DataType.Text)]
        public short YAxis { get; set; }

        /// <summary>
        /// New up an empty model plane
        /// </summary>
        [JsonConstructor]
        public DimensionalModelPlane()
        {
            ModelNodes = new HashSet<IDimensionalModelNode>();
        }

        public DimensionalModelPlane(bool emptyNodes = true)
        {
            ModelNodes = new HashSet<IDimensionalModelNode>();

            for (int i = 0; i < 21; i++)
            {
                ModelNodes.Add(new DimensionalModelNode());
            }
        }

        /// <summary>
        /// Gets a node based on the X and Y axis
        /// </summary>
        /// <param name="xAxis">the X-Axis of the node to get</param>
        /// <returns>the node</returns>
        public IDimensionalModelNode GetNode(short xAxis)
        {
            return ModelNodes.FirstOrDefault(node => node.XAxis.Equals(xAxis));
        }
    }
}
