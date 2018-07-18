using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.SupportingClasses;

namespace NetMud.DataStructure.Behaviors.Rendering
{
    /// <summary>
    /// Rendering methods for when a location that contains the entity being rendered is being rendered
    /// </summary>
    public interface IRenderInLocation
    {
        /// <summary>
        /// Renders output for this entity when Look targets the container it is in
        /// </summary>
        /// <param name="viewer">entity initiating the command</param>
        /// <returns>the output</returns>
        IOccurrence RenderAsContents(IEntity viewer);
    }
}
