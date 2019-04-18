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
        [Display(Name = "Stamina Range", Description = "Does the target need to have at least X stamina or at most X stamina to use?.")]
        [UIHint("ValueRangeInt")]
        public ValueRange<int> StaminaRange { get; set; }

        /// <summary>
        /// Does the target need to have at least X health or at most X health to use?
        /// </summary>
        [Display(Name = "Health Range", Description = "Does the target need to have at least X health or at most X health to use?.")]
        [UIHint("ValueRangeInt")]
        public ValueRange<int> HealthRange { get; set; }

        /// <summary>
        /// What position does the target need to be in
        /// </summary>
        [Display(Name = "Valid Position", Description = " What position does the person need to be in.")]
        [UIHint("EnumDropDownList")]
        public MobilityState ValidPosition { get; set; }

        /// <summary>
        /// The quality we're checking for
        /// </summary>
        [Display(Name = "Quality", Description = "Name of quality required before this is usable.")]
        [DataType(DataType.Text)]
        public string Quality { get; set; }

        /// <summary>
        /// The value range of the quality we're checking for
        /// </summary>
        [Display(Name = "Quality Value Range", Description = "Value range for the high and low caps for the required quality.")]
        [UIHint("ValueRangeInt")]
        public ValueRange<int> QualityRange { get; set; }

        public FightingArtCriteria()
        {
            ValidPosition = MobilityState.Standing;
            HealthRange = new ValueRange<int>(1, -1);
            StaminaRange = new ValueRange<int>(1, -1);
            QualityRange = new ValueRange<int>(0, 0);
        }

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
