using NetMud.DataStructure.Base.System;
using System.Collections.Generic;
namespace NetMud.DataStructure.Behaviors.Rendering
{
    /// <summary>
    /// Indicates something is heard and affects audible triggers
    /// </summary>
    public interface IAudible
    {        
        /// <summary>
        /// Renders "display" from scan command
        /// </summary>
        /// <param name="actor">entity initiating the command</param>
        /// <returns>the scan output</returns>
        IEnumerable<string> RenderToAudible(IEntity actor);
    }
}
