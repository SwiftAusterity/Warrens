using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Architectural.EntityBase
{
    /// <summary>
    /// Framework for rendering output when this entity is Look(ed) at
    /// </summary>
    public interface ILookable
    {
        /// <summary>
        /// Renders output for this entity when Look targets it
        /// </summary>
        /// <param name="viewer">entity initiating the command</param>
        /// <returns>the output</returns>
        IEnumerable<IMessage> RenderToLook(IEntity viewer);

        /// <summary>
        /// Retrieve all of the descriptors that are tagged as Tactile output
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        IEnumerable<ISensoryEvent> GetVisibleDescriptives(IEntity viewer);

        /// <summary>
        /// Is this thing sensible to the entity
        /// </summary>
        /// <param name="actor">the observing entity</param>
        /// <returns>0 = observable, negative = too low to detect, positive = too high to detect</returns>
        short GetVisibleDelta(IEntity actor);
    }
}
