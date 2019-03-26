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
        ILexicalParagraph RenderToOlefactory(IEntity actor);

        /// <summary>
        /// Retrieve all of the descriptors that are tagged as Olefactory output
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        IEnumerable<ISensoryEvent> GetOlefactoryDescriptives(IEntity viewer);

        /// <summary>
        /// Is this thing sensible to the entity
        /// </summary>
        /// <param name="actor">the observing entity</param>
        /// <returns>(-100) to 100 rating of how well this can be detected. 0 is full detection. negative is too "low", over 0 is too "intense"</returns>
        short GetOlefactoryDelta(IEntity actor, short modifier = 0);
    }
}
