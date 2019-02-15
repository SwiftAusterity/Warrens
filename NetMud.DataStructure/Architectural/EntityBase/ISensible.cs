using System.Collections.Generic;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.System;

namespace NetMud.DataStructure.Architectural.EntityBase
{
    /// <summary>
    /// Indicates a command can output sense (psychic) output
    /// </summary>
    public interface ISensible : IDescribable
    {
        /// <summary>
        /// Renders output for this entity when Look targets it
        /// </summary>
        /// <param name="actor">entity initiating the command</param>
        /// <returns>the output</returns>
        IMessage RenderToSense(IEntity actor);

        /// <summary>
        /// Retrieve all of the descriptors that are tagged as Psychic output
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        IEnumerable<ISensoryEvent> GetPsychicDescriptives(IEntity viewer);

        /// <summary>
        /// Is this thing sensible to the entity
        /// </summary>
        /// <param name="actor">the observing entity</param>
        /// <returns>If this is observeable</returns>
        bool IsSensibleTo(IEntity actor);
    }
}
