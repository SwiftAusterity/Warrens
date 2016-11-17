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

        /// <summary>
        /// How soluble the dirt is
        /// </summary>
        int Solubility { get; set; }

        /// <summary>
        /// How fertile the dirt generally is
        /// </summary>
        int Fertility { get; set; }

        /// <summary>
        /// What medium materials this can spawn in
        /// </summary>
        IEnumerable<IMineral> Ores { get; set; }
    }
}
