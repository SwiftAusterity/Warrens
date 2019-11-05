using NetMud.DataStructure.Architectural;
using System.Collections.Generic;

namespace NetMud.DataStructure.Linguistic
{
    public interface IDictataPhrase : IConfigData, IWeightedMeaning
    {
        /// <summary>
        /// Things this is the same as mostly
        /// </summary>
        HashSet<IDictata> Words { get; set; }
    }
}
