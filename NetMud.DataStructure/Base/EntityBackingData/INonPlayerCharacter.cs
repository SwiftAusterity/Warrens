using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.SupportingClasses;
using System.Collections.Generic;

namespace NetMud.DataStructure.Base.EntityBackingData
{
    /// <summary>
    /// Backing data for NPC/Intelligences
    /// </summary>
    public interface INonPlayerCharacter : IEntityBackingData, IGender
    {
        /// <summary>
        /// Family name for NPCs
        /// </summary>
        string SurName { get; set; }

        /// <summary>
        /// Given + family name for NPCs
        /// </summary>
        /// <returns></returns>
        string FullName();
    }
}
