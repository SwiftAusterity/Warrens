using NetMud.Data.Architectural;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Combat;
using NetMud.DataStructure.Player;
using NetMud.Utility;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Script.Serialization;

namespace NetMud.Data.Combat
{
    [Serializable]
    public class FightingArt : LookupDataPartial, IFightingArt
    {
        /// <summary>
        /// What type of approval is necessary for this content
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public override ContentApprovalType ApprovalType { get { return ContentApprovalType.Staff; } }

        /// <summary>
        /// How much stam this takes/damages
        /// </summary>
        [Display(Name = "Stamina Cost/Damage", Description = "Stamina Cost / Damage.")]
        [UIHint("ValuePairInt")]
        public ValuePair<int> Stamina { get; set; }

        /// <summary>
        /// How much health this costs/damages the actor/victim
        /// </summary>
        [Display(Name = "Health Cost/Damage", Description = "Health Cost / Damage.")]
        [UIHint("ValuePairInt")]
        public ValuePair<int> Health { get; set; }

        /// <summary>
        /// How much stagger this has when it hits
        /// </summary>
        [Display(Name = "Stagger", Description = "How much stagger this has when it hits.")]
        [DataType(DataType.Text)]
        public int Stagger { get; set; }

        /// <summary>
        /// Results in actor/victim position change
        /// </summary>
        [Display(Name = "Position Result", Description = "esults in actor/victim position change.")]
        [UIHint("ValuePairMobilityState")]
        public ValuePair<MobilityState> PositionResult { get; set; }

        /// <summary>
        /// The min and max distance this is usable
        /// </summary>
        [Display(Name = "Distance Range", Description = "The min and max distance this is usable.")]
        [UIHint("ValueRangeULong")]
        public ValueRange<ulong> DistanceRange { get; set; }

        /// <summary>
        /// How should this alter the combatent distance
        /// </summary>
        [Display(Name = "Distance Change", Description = "How should this alter the combatent distance.")]
        [DataType(DataType.Text)]
        public int DistanceChange { get; set; }

        /// <summary>
        /// How many action frames this takes to execute from init before the hit
        /// </summary>
        [Display(Name = "Setup", Description = "How many action frames this takes to execute from init before the hit.")]
        [DataType(DataType.Text)]
        public int Setup { get; set; }

        /// <summary>
        /// How many action frames this takes to execute after the hit to end
        /// </summary>
        [Display(Name = "Recovery", Description = "How many action frames this takes to execute after the hit to end.")]
        [DataType(DataType.Text)]
        public int Recovery { get; set; }

        /// <summary>
        /// How many frames this adds to the recovery of the Actor when blocked and how much additional stagger it does when blocked
        /// </summary>
        [Display(Name = "Impact", Description = "How many frames this adds to the recovery of the Actor when blocked and how much additional stagger it does when blocked.")]
        [DataType(DataType.Text)]
        public int Impact { get; set; }

        /// <summary>
        /// How much stagger-armor does this have while executing (reduces incoming stagger directly, does not reduce stagger costs)
        /// </summary>
        [Display(Name = "Armor", Description = "How much stagger-armor does this have while executing (reduces incoming stagger directly, does not reduce stagger costs)")]
        [DataType(DataType.Text)]
        public int Armor { get; set; }

        /// <summary>
        /// State of readiness this art puts the user in during its duration
        /// </summary>
        [Display(Name = "Readiness", Description = "State of readiness this art puts the user in during its duration.")]
        [UIHint("EnumDropDownList")]
        public ReadinessState Readiness { get; set; }

        /// <summary>
        /// Is this a part of a multipart attack
        /// </summary>
        [Display(Name = "Rekka", Description = "Name of multipart attack.")]
        [DataType(DataType.Text)]
        public string RekkaKey { get; set; }

        /// <summary>
        /// Where in the multipart attack does this go
        /// </summary>
        [Display(Name = "Rekka Position", Description = "Where in the multipart attack does this go.")]
        [DataType(DataType.Text)]
        public int RekkaPosition { get; set; }

        /// <summary>
        /// Where does this move aim
        /// </summary>
        [Display(Name = "Aim", Description = "Where does this move aim.")]
        [UIHint("EnumDropDownList")]
        public AnatomyAim Aim { get; set; }

        /// <summary>
        /// Criteria for usage for actor
        /// </summary>
        [Display(Name = "Actor Criteria", Description = "Criteria for the one executing the Art.")]
        [UIHint("FightingArtCriteria")]
        public IFightingArtCriteria ActorCriteria { get; set; }

        /// <summary>
        /// Criteria for usage for victim
        /// </summary>
        [Display(Name = "Victim Criteria", Description = "Criteria for the one being targeted by the Art.")]
        [UIHint("FightingArtCriteria")]
        public IFightingArtCriteria VictimCriteria { get; set; }

        /// <summary>
        /// The quality we're checking for
        /// </summary>
        [Display(Name = "Quality", Description = "Name of quality this adds to on use.")]
        [DataType(DataType.Text)]
        public string ResultQuality { get; set; }

        /// <summary>
        /// Is this quality additive or replace
        /// </summary>
        [Display(Name = "Additive", Description = "Does this quality add to an existing quality (true) or replace the value(false).")]
        [UIHint("Boolean")]
        public bool AdditiveQuality { get; set; }

        /// <summary>
        /// The value we're adding to the quality
        /// </summary>
        [Display(Name = "Amount", Description = "How much is added for the applied quality.")]
        [DataType(DataType.Text)]
        public int QualityValue { get; set; }

        public FightingArt()
        {
            QualityValue = 0;
            AdditiveQuality = true;
            Aim = AnatomyAim.Mid;
            PositionResult = new ValuePair<MobilityState>(MobilityState.None, MobilityState.None);
            Stamina = new ValuePair<int>(1, 0);
            Health = new ValuePair<int>(0, 1);
            Stagger = 1;
            Impact = 1;
            Armor = 0;
            Setup = 1;
            Recovery = 1;
            RekkaPosition = -1;
            ActorCriteria = new FightingArtCriteria();
            VictimCriteria = new FightingArtCriteria();
            Readiness = ReadinessState.Offensive;
        }

        /// <summary>
        /// Is this art valid to be used at the moment
        /// </summary>
        /// <param name="actor">who's doing the hitting</param>
        /// <param name="victim">who's being hit</param>
        /// <returns>yea or nay</returns>
        public bool IsValid(IPlayer actor, IPlayer victim, ulong distance, IFightingArt lastAttack = null)
        {
            return distance.IsBetweenOrEqual(DistanceRange.Low, DistanceRange.High)
                && actor.CurrentHealth >= (ulong)Health.Actor
                && actor.CurrentStamina >= Stamina.Actor
                && (lastAttack == null || (lastAttack.RekkaKey.Equals(RekkaKey) && lastAttack.RekkaPosition == RekkaPosition - 1));
        }
    }
}