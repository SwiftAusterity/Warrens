using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Tile;
using System.Collections.Generic;

namespace NetMud.DataStructure.System
{
    /// <summary>
    /// Singular message object for output parsing
    /// </summary>
    public interface IMessage
    {

        /// <summary>
        /// Overrides the grammatical generator
        /// </summary>
        IEnumerable<string> Body { get; set; }

        /// <summary>
        /// Will this message been seen/heard/etc by the target
        /// </summary>
        /// <returns>if the message will be noticed</returns>
        bool IsNoticed(IEntity subject, IEntity target, ITile origin);
    }
}
