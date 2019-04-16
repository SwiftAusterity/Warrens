using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Combat;
using NetMud.DataStructure.Player;
using NetMud.Utility;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Data.Combat
{
    public class FightingArtCriteria : IFightingArtCriteria
    {
        /// <summary>
        /// Does the target need to have at least X health or at most X stamina to use?
        /// </summary>
        [Display(Name = "Distance Range", Description = "The min and max distance this is usable.")]
        [UIHint("ValueRangeInt")]
        public ValueRange<int> StaminaRange { get; set; }

        /// <summary>
        /// Does the target need to have at least X health or at most X health to use?
        /// </summary>
        [Display(Name = "Distance Range", Description = "The min and max distance this is usable.")]
        [UIHint("ValueRangeInt")]
        public ValueRange<int> HealthRange { get; set; }

        /// <summary>
        /// What position does the target need to be in
        /// </summary>
        [Display(Name = "Valid Position", Description = " What position does the person need to be in.")]
        [UIHint("EnumDropDownList")]
        public MobilityState ValidPosition { get; set; }

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
                && target.StancePosition == ValidPosition;
        }
    }
}
