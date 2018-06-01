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
        /// <param name="teller">entity describer</param>
        /// <param name="reciever">entity being described to</param>
        /// <returns>the scan output</returns>
        IEnumerable<string> DescribeTo(IEntity teller, IEntity reciever);
    }
}
