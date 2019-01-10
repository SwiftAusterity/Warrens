﻿using NetMud.DataStructure.Architectural.EntityBase;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.DataStructure.NPC
{
    /// <summary>
    /// Ensures NPCs have stock for being merchants
    /// </summary>
    public interface IHaveSkillsToTeach
    {
        /// <summary>
        /// Qualities this teacher can impart, the quality value is the max level it can be taught to (1 at a time)
        /// </summary>
        [UIHint("TeachableProficency")]
        HashSet<IQuality> TeachableProficencies { get; set; }
    }

}
