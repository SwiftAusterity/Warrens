using System.Collections.Generic;

namespace NetMud.DataStructure.NPC.IntelligenceControl
{
    /// <summary>
    /// The memory version of an Actor entity
    /// </summary>
    public interface IReflection
    {
        /// <summary>
        /// The name of the thing
        /// </summary>
        string FullName { get; set; }

        /// <summary>
        /// Its qualities
        /// </summary>
        Dictionary<string, short> Features { get; set; }

        /// <summary>
        /// The hex color of the actor
        /// </summary>
        string AppearanceHexColor { get; set; }

        /// <summary>
        /// The "physical appearance" of the thing
        /// </summary>
        string AppearanceCharacter { get; set; }
    }
}
