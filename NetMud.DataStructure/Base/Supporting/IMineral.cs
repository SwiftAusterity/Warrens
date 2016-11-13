using NetMud.DataStructure.Behaviors.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.DataStructure.Base.Supporting
{
    /// <summary>
    /// Rocks, minable metals and dirt
    /// </summary>
    public interface IMineral : INaturalResource
    {
        /// <summary>
        /// What is the solid, crystallized form of this
        /// </summary>
        IMaterial Rock { get; set; }

        /// <summary>
        /// What is the scattered, ground form of this
        /// </summary>
        IMaterial Dirt { get; set; }
    }
}
