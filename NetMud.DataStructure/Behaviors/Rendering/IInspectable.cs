using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.SupportingClasses;

namespace NetMud.DataStructure.Behaviors.Rendering
{
    /// <summary>
    /// Indicates an entity can be Inspected (part of rendering)
    /// </summary>
    public interface IInspectable
    {
        /// <summary>
        /// Renders "display" from scan command
        /// </summary>
        /// <param name="viewer">entity initiating the command</param>
        /// <returns>the scan output</returns>
        IOccurrence RenderToInspect(IEntity viewer);
    }
}
