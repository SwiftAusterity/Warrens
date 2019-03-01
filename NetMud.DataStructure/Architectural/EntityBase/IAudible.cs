using System.Collections.Generic;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.System;

namespace NetMud.DataStructure.Architectural.EntityBase
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
        IMessage RenderToAudible(IEntity actor);

        /// <summary>
        /// Retrieve all of the descriptors that are tagged as Audible output
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        IEnumerable<ISensoryEvent> GetAudibleDescriptives(IEntity viewer);

        /// <summary>
        /// Is this thing sensible to the entity
        /// </summary>
        /// <param name="actor">the observing entity</param>
        /// <returns>0 = observable, negative = too low to detect, positive = too high to detect</returns>
        short GetAudibleDelta(IEntity actor);
    }
}
