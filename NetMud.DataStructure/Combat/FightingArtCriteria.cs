using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Architectural.EntityBase;
using System.Collections.Generic;

namespace NetMud.DataStructure.Combat
{
    public class FightingArtCriteria
    {
        /// <summary>
        /// Does the target need to have at least X health or at most X stamina to use?
        /// </summary>
        public ValueRange<int> StaminaRange { get; set; }

        /// <summary>
        /// Does the target need to have at least X health or at most X health to use?
        /// </summary>
        public ValueRange<int> HealthRange { get; set; }

        /// <summary>
        /// What stance does the target need to be in
        /// </summary>
        public HashSet<MobilityState> ValidStances { get; set; }

        /// <summary>
        /// The min and max distance this is usable
        /// </summary>
        public ValueRange<ulong> Distance { get; set; }

        /// <summary>
        /// Validate the criteria against the actor and victim
        /// </summary>
        /// <param name="actor">who's doing the hitting</param>
        /// <param name="victim">who's being hit</param>
        /// <returns></returns>
        public bool Validate(IEntity actor, IEntity victim)
        {
            return true;
        }
    }
}
