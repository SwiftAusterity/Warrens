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
        /// How much stagger this has when it hits
        /// </summary>
        int Stagger { get; set; }

        /// <summary>
        /// Results in actor/victim stance change
        /// </summary>
        ValuePair<MobilityState> StanceResult { get; set; }

        /// <summary>
        /// The min and max distance this is usable
        /// </summary>
        ValueRange<ulong> DistanceRange { get; set; }

        /// <summary>
        /// How should this alter the combatent distance
        /// </summary>
        ulong DistanceChange { get; set; }

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
        /// Is this art valid to be used at the moment
        /// </summary>
        /// <param name="actor">who's doing the hitting</param>
        /// <param name="victim">who's being hit</param>
        /// <returns>yea or nay</returns>
        bool IsValid(IPlayer actor, IPlayer victim, ulong distance, IFightingArt lastAttack = null);
    }
}
