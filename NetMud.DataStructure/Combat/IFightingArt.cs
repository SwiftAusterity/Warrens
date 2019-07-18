using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Player;

namespace NetMud.DataStructure.Combat
{
    public interface IFightingArt : ILookupData
    {
        /// <summary>
        /// How much stam this takes
        /// </summary>
        ValuePair<int> Stamina { get; set; }

        /// <summary>
        /// How much health this costs the actor
        /// </summary>
        ValuePair<int> Health { get; set; }

        /// <summary>
        /// Results in actor/victim position change
        /// </summary>
        ValuePair<MobilityState> PositionResult { get; set; }

        /// <summary>
        /// The min and max distance this is usable
        /// </summary>
        ValueRange<ulong> DistanceRange { get; set; }

        /// <summary>
        /// How should this alter the combatent distance
        /// </summary>
        int DistanceChange { get; set; }

        /// <summary>
        /// How many action frames this takes to execute from init before the hit
        /// </summary>
        int Setup { get; set; }

        /// <summary>
        /// How many action frames this takes to execute after the hit to end
        /// </summary>
        int Recovery { get; set; }

        /// <summary>
        /// How many frames this adds to the recovery of the Actor when blocked and how much additional stagger it does when blocked
        /// </summary>
        int Impact { get; set; }

        /// <summary>
        /// How much stagger-armor does this have while executing (reduces incoming stagger directly, does not reduce stagger costs)
        /// </summary>
        int Armor { get; set; }

        /// <summary>
        /// State of readiness this art puts the user in during its duration
        /// </summary>
        ReadinessState Readiness { get; set; }

        /// <summary>
        /// Is this a part of a multipart attack
        /// </summary>
        string RekkaKey { get; set; }

        /// <summary>
        /// Where in the multipart attack does this go
        /// </summary>
        int RekkaPosition { get; set; }

        /// <summary>
        /// Where does this move aim
        /// </summary>
        AnatomyAim Aim { get; set; }

        /// <summary>
        /// Criteria for usage for actor
        /// </summary>
        IFightingArtCriteria ActorCriteria { get; set; }

        /// <summary>
        /// Criteria for usage for victim
        /// </summary>
        IFightingArtCriteria VictimCriteria { get; set; }

        /// <summary>
        /// The quality we're checking for
        /// </summary>
        string ResultQuality { get; set; }

        /// <summary>
        /// Is this quality additive or replace
        /// </summary>
        bool AdditiveQuality { get; set; }

        /// <summary>
        /// The value we're adding to the quality
        /// </summary>
        int QualityValue { get; set; }

        /// <summary>
        /// The verb of the sentence for output building
        /// </summary>
        string ActionVerb { get; set; }

        /// <summary>
        /// The subject of the sentence for output building
        /// </summary>
        string ActionSubject { get; set; }

        /// <summary>
        /// The predicate of the sentence for output building
        /// </summary>
        string ActionPredicate { get; set; }

        /// <summary>
        /// Is this art valid to be used at the moment
        /// </summary>
        /// <param name="actor">who's doing the hitting</param>
        /// <param name="victim">who's being hit</param>
        /// <returns>yea or nay</returns>
        bool IsValid(IMobile actor, IMobile victim, ulong distance, IFightingArt lastAttack = null);

        /// <summary>
        /// Calculate the cost ratio of this art
        /// </summary>
        /// <returns></returns>
        double CalculateCostRatio();

    }
}
