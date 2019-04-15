using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Combat;
using NetMud.DataStructure.Player;
using NetMud.Utility;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.Data.Combat
{
    public class FightingArtCriteria : IFightingArtCriteria
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
        public HashSet<MobilityState> ValidPositions { get; set; }

        /// <summary>
        /// The min and max distance this is usable
        /// </summary>
        public ValueRange<ulong> DistanceRange { get; set; }

        /// <summary>
        /// Validate the criteria against the actor and victim
        /// </summary>
        /// <param name="actor">who's doing the hitting</param>
        /// <param name="victim">who's being hit</param>
        /// <returns></returns>
        public bool Validate(IPlayer target, ulong distance)
        {
            return target.CurrentStamina.IsBetweenOrEqual(StaminaRange.Low, StaminaRange.High)
                && target.CurrentHealth.IsBetweenOrEqual(HealthRange.Low, HealthRange.High)
                && (ValidPositions.Count() == 0 || ValidPositions.Contains(target.StancePosition))
                && distance.IsBetweenOrEqual(DistanceRange.Low, DistanceRange.High);
        }
    }
}
