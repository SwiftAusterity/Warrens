using System;
using System.Collections.Generic;

namespace NetMud.DataStructure.NPC.IntelligenceControl
{
    public interface IBrain
    {
        /// <summary>
        /// Cluster of needs and priority
        /// </summary>
        HashSet<Tuple<Motivator, int>> Needs { get; set; }

        /// <summary>
        /// Get the need that is currently the most pressing
        /// </summary>
        Motivator HighestCurrentNeed { get; }

        /// <summary>
        /// Add pressure to a need, can also be negative, wont go below zero
        /// </summary>
        /// <param name="need">the need to apply pressure to</param>
        /// <param name="amount">how much pressure</param>
        /// <returns>the highest current need after application</returns>
        Motivator ApplyPressure(Motivator need, int amount);

        /// <summary>
        /// Get an activity that will meet the goal
        /// </summary>
        /// <param name="motivator">the goal at hand</param>
        /// <returns>an activity to meet it</returns>
        Accomplisher HowToDo(Motivator motivator);
    }
}
