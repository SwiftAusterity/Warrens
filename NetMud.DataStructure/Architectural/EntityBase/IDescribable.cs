using NetMud.DataStructure.Linguistic;
using System.Collections.Generic;

namespace NetMud.DataStructure.Architectural.EntityBase
{ 
    /// <summary>
    /// Indicates a data structure has additional descriptives, is part of rendering
    /// </summary>
    public interface IDescribable
    {
        /// <summary>
        /// Set of output relevant to this exit
        /// </summary>
        HashSet<IOccurrence> Descriptives { get; set; }
    }
}
