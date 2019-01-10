using NetMud.DataStructure.NPC.IntelligenceControl;
using System;
using System.Collections.Generic;

namespace NetMud.Data.NPC.IntelligenceControl
{
    /// <summary>
    /// A record of NPC observance
    /// </summary>
    [Serializable]
    public class Reflection : IReflection
    {
        /// <summary>
        /// The name of the thing
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Its qualities
        /// </summary>
        public Dictionary<string, short> Features { get; set; }

        /// <summary>
        /// The hex color of the actor
        /// </summary>
        public string AppearanceHexColor { get; set; }

        /// <summary>
        /// The "physical appearance" of the thing
        /// </summary>
        public string AppearanceCharacter { get; set; }
    }

}
