using System.Collections.Generic;
using NetMud.DataStructure.Linguistic;

namespace NetMud.DataStructure.Architectural.EntityBase
{
    /// <summary>
    /// Framework for rendering Smell output
    /// </summary>
    public interface ISmellable : IDescribable
    {
        /// <summary>
        /// Renders output for this entity when Look targets it
        /// </summary>
        /// <param name="actor">entity initiating the command</param>
        /// <returns>the output</returns>
        ILexicalParagraph RenderToSmell(IEntity actor);

        /// <summary>
        /// Retrieve all of the descriptors that are tagged as Olefactory output
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        IEnumerable<ISensoryEvent> GetSmellableDescriptives(IEntity viewer);

        /// <summary>
        /// Is this thing sensible to the entity
        /// </summary>
        /// <param name="actor">the observing entity</param>
        /// <returns>0 = observable, negative = too low to detect, positive = too high to detect</returns>
        short GetSmellDelta(IEntity actor);
    }
}
