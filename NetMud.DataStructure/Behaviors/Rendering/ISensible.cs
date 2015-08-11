using NetMud.DataStructure.Base.System;
using System.Collections.Generic;
namespace NetMud.DataStructure.Behaviors.Rendering
{
    /// <summary>
    /// Indicates a command can output sense (psychic) output
    /// </summary>
    public interface ISensible
    {
        /// <summary>
        /// Renders output for this entity when Look targets it
        /// </summary>
        /// <param name="actor">entity initiating the command</param>
        /// <returns>the output</returns>
        IEnumerable<string> RenderToSense(IEntity actor);
    }
}
