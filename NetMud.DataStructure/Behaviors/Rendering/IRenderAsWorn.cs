using NetMud.DataStructure.Base.System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Behaviors.Rendering
{
    /// <summary>
    /// For when something that can be worn is looked at as being worn
    /// </summary>
    public interface IRenderAsWorn
    {
        /// <summary>
        /// Renders output for this entity when it is held by something they are looking at
        /// </summary>
        /// <param name="viewer">entity initiating the command</param>
        /// <param name="wearer">entity wearing the item</param>
        /// <returns>the output</returns>
        IEnumerable<string> RenderAsWorn(IEntity viewer, IEntity wearer);
    }
}
