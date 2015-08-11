using NetMud.DataStructure.Base.System;
using System.Collections.Generic;
namespace NetMud.DataStructure.Behaviors.Rendering
{
    /// <summary>
    /// Framework for rendering Scan command output for an entity being scanned
    /// </summary>
    public interface IScanable
    {
        /// <summary>
        /// Renders "display" from scan command
        /// </summary>
        /// <param name="actor">entity initiating the command</param>
        /// <returns>the scan output</returns>
        IEnumerable<string> RenderToScan(IEntity actor);
    }
}
