using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Linguistic;

namespace NetMud.DataStructure.Inanimate
{
    /// <summary>
    /// For when something that can be held is looked at as being held
    /// </summary>
    public interface IRenderAsWorn
    {
        /// <summary>
        /// Renders output for this entity when it is held by something they are looking at
        /// </summary>
        /// <param name="viewer">entity initiating the command</param>
        /// <param name="holder">entity holding the thing</param>
        /// <returns>the output</returns>
        ISensoryEvent RenderAsWorn(IEntity viewer, IEntity holder);
    }
}
