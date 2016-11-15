﻿using NetMud.DataStructure.Behaviors.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.DataStructure.Base.Supporting
{
    /// <summary>
    /// Animal spawns
    /// </summary>
    interface IFauna : INaturalResource
    {
        /// <summary>
        /// What we're spawning
        /// </summary>
        IRace Race { get; set; }

        /// <summary>
        /// What is the % chance of generating a female instead of a male on birth
        /// </summary>
        int FemaleRatio { get; set; }

        /// <summary>
        /// The absolute hard cap to natural population growth
        /// </summary>
        int PopulationHardCap { get; set; }
    }
}