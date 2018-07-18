using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.SupportingClasses;
using System.Collections.Generic;

namespace NetMud.DataStructure.Behaviors.Rendering
{
    /// <summary>
    /// Framework for rendering Taste output
    /// </summary>
    public interface ITasteable : IDescribable
    {
        /// <summary>
        /// Renders output for this entity when Look targets it
        /// </summary>
        /// <param name="actor">entity initiating the command</param>
        /// <returns>the output</returns>
        IOccurrence RenderToTaste(IEntity actor);

        /// <summary>
        /// Retrieve all of the descriptors that are tagged as Taste output
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        IEnumerable<IOccurrence> GetTasteDescriptives();

        /// <summary>
        /// Is this thing sensible to the entity
        /// </summary>
        /// <param name="actor">the observing entity</param>
        /// <returns>If this is observeable</returns>
        bool IsTastableTo(IEntity actor);
    }
}
