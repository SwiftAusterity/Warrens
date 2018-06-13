using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.SupportingClasses;
using System.Collections.Generic;
namespace NetMud.DataStructure.Behaviors.Rendering
{
    /// <summary>
    /// Indicates something is heard and affects audible triggers
    /// </summary>
    public interface IAudible : IDescribable
    {        
        /// <summary>
        /// Renders "display" from scan command
        /// </summary>
        /// <param name="actor">entity initiating the command</param>
        /// <returns>the scan output</returns>
        IEnumerable<string> RenderToAudible(IEntity actor);

        /// <summary>
        /// Retrieve all of the descriptors that are tagged as Audible output
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        IEnumerable<IOccurrence> GetAudibleDescriptives();
    }
}
