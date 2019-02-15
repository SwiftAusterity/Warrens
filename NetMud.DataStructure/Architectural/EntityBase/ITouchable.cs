using System.Collections.Generic;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.System;

namespace NetMud.DataStructure.Architectural.EntityBase
{
    /// <summary>
    /// Framework for rendering Tactile output
    /// </summary>
    public interface ITouchable : IDescribable
    {
        /// <summary>
        /// Renders output for this entity when Look targets it
        /// </summary>
        /// <param name="actor">entity initiating the command</param>
        /// <returns>the output</returns>
        IMessage RenderToTouch(IEntity actor);

        /// <summary>
        /// Retrieve all of the descriptors that are tagged as Tactile output
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        IEnumerable<ISensoryEvent> GetTouchDescriptives(IEntity viewer);

        /// <summary>
        /// Is this thing sensible to the entity
        /// </summary>
        /// <param name="actor">the observing entity</param>
        /// <returns>If this is observeable</returns>
        bool IsTouchableTo(IEntity actor);
    }
}
