using NetMud.DataStructure.Base.System;
using System.Collections.Generic;
namespace NetMud.DataStructure.Behaviors.Rendering
{
    /// <summary>
    /// Indicates an entity can be Describe(d) to another entity, is part of rendering
    /// </summary>
    public interface IDescribable
    {
        /// <summary>
        /// Renders "display" from scan command
        /// </summary>
        /// <param name="actor">entity initiating the command</param>
        /// <returns>the scan output</returns>
        IEnumerable<string> DescribeTo(IEntity actor, IEntity target);
    }
}
