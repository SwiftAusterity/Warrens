using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Player;
using NetMud.Utility;
using System;

namespace NetMud.DataStructure.Combat
{
    [Serializable]
    public class FightingArt : IFightingArt
    {
        /// <summary>
        /// How much stam this takes/damages
        /// </summary>
        public ValuePair<int> Stamina { get; set; }

        /// <summary>
        /// How much health this costs/damages the actor/victim
        /// </summary>
        public ValuePair<int> Health { get; set; }

        /// <summary>
        /// How much stagger this has when it hits
        /// </summary>
        public int Stagger { get; set; }

        /// <summary>
        /// Results in actor/victim stance change
        /// </summary>
        public ValuePair<MobilityState> StanceResult { get; set; }

        /// <summary>
        /// The min and max distance this is usable
        /// </summary>
        public ValueRange<ulong> DistanceRange { get; set; }

        /// <summary>
        /// How should this alter the combatent distance
        /// </summary>
        public ulong DistanceChange { get; set; }

        /// <summary>
        /// How many action frames this takes to execute from init before the hit
        /// </summary>
        public int Setup { get; set; }

        /// <summary>
        /// How many action frames this takes to execute after the hit to end
        /// </summary>
        public int Recovery { get; set; }

        /// <summary>
        /// How many frames this adds to the recovery of the Actor when blocked and how much additional stagger it does when blocked
        /// </summary>
        public int Impact { get; set; }

        /// <summary>
        /// How much stagger-armor does this have while executing (reduces incoming stagger directly, does not reduce stagger costs)
        /// </summary>
        public int Armor { get; set; }

        /// <summary>
        /// Is this a part of a multipart attack
        /// </summary>
        public string RekkaKey { get; set; }

        /// <summary>
        /// Where in the multipart attack does this go
        /// </summary>
        public int RekkaPosition { get; set; }

        /// <summary>
        /// Where does this move aim
        /// </summary>
        public AnatomyAim Aim { get; set; }

        /// <summary>
        /// Criteria for usage for actor
        /// </summary>
        public IFightingArtCriteria ActorCriteria { get; set; }

        /// <summary>
        /// Criteria for usage for victim
        /// </summary>
        public IFightingArtCriteria VictimCriteria { get; set; }

        /// <summary>
        /// Is this art valid to be used at the moment
        /// </summary>
        /// <param name="actor">who's doing the hitting</param>
        /// <param name="victim">who's being hit</param>
        /// <returns>yea or nay</returns>
        public bool IsValid(IPlayer actor, IPlayer victim, ulong distance, IFightingArt lastAttack = null)
        {
            return distance.IsBetweenOrEqual(DistanceRange.Low, DistanceRange.High)
                && actor.CurrentHealth >= Health.Actor 
                && actor.CurrentStamina >= Stamina.Actor
                && (lastAttack == null || (lastAttack.RekkaKey.Equals(RekkaKey) && lastAttack.RekkaPosition == RekkaPosition -1));
        }
    }
}
