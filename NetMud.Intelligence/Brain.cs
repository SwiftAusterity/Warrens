using NetMud.DataStructure.NPC.IntelligenceControl;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.Intelligence
{
    /// <summary>
    /// The current mental state of an AI
    /// </summary>
    [Serializable]
    public class Brain : IBrain
    {
        /// <summary>
        /// Cluster of needs and priority
        /// </summary>
        public HashSet<Tuple<Motivator, int>> Needs { get; set; }

        /// <summary>
        /// Get the need that is currently the most pressing
        /// </summary>
        public Motivator HighestCurrentNeed => Needs.Count() > 0 ? Needs.OrderByDescending(nd => nd.Item2).FirstOrDefault().Item1 : Motivator.Lonliness;

        public Brain()
        {
            Needs = new HashSet<Tuple<Motivator, int>>();
        }

        /// <summary>
        /// Add pressure to a need, can also be negative, wont go below zero
        /// </summary>
        /// <param name="need">the need to apply pressure to</param>
        /// <param name="amount">how much pressure</param>
        /// <returns>the highest current need after application</returns>
        public Motivator ApplyPressure(Motivator need, int amount)
        {
            Tuple<Motivator, int> currentNeed = Needs.FirstOrDefault(nd => nd.Item1 == need);

            if (currentNeed != null)
            {
                amount += currentNeed.Item2;
                Needs.Remove(currentNeed);
            }

            Needs.Add(new Tuple<Motivator, int>(need, Math.Max(0, amount)));

            return Needs.OrderByDescending(nd => nd.Item2).FirstOrDefault().Item1;
        }

        /// <summary>
        /// Get an activity that will meet the goal
        /// </summary>
        /// <param name="motivator">the goal at hand</param>
        /// <returns>an activity to meet it</returns>
        public Accomplisher HowToDo(Motivator motivator)
        {
            short motivation = (short)motivator;
            Accomplisher todo = Accomplisher.Wander;
            Array activities = Enum.GetValues(typeof(Accomplisher));

            if (activities.Length >= motivation)
            {
                todo = (Accomplisher)motivation;
            }

            return todo;
        }
    }
}
