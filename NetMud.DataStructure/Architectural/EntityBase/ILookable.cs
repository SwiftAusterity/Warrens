using NetMud.DataStructure.Linguistic;
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
        ILexicalParagraph RenderToVisible(IEntity viewer);

        /// <summary>
        /// Retrieve all of the descriptors that are tagged as Tactile output
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        IEnumerable<ISensoryEvent> GetVisibleDescriptives(IEntity viewer);

        /// <summary>
        /// Is this thing sensible to the entity
        /// </summary>
        /// <param name="actor">the observing entity</param>
        /// <returns>(-100) to 100 rating of how well this can be detected. 0 is full detection. negative is too "low", over 0 is too "intense"</returns>
        short GetVisibleDelta(IEntity actor, short modifier = 0);
    }
}
