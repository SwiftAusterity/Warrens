using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.SupportingClasses;
using System.Collections.Generic;

namespace NetMud.DataStructure.Behaviors.Rendering
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
