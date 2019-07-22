using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;

namespace NetMud.DataStructure.Combat
{
    public interface IFightingArtCriteria
    {
        /// <summary>
        /// Does the target need to have at least X health or at most X stamina to use?
        /// </summary>
        ValueRange<int> StaminaRange { get; set; }

        /// <summary>
        /// Does the target need to have at least X health or at most X health to use?
        /// </summary>
        ValueRange<int> HealthRange { get; set; }

        /// <summary>
        /// What position does the target need to be in
        /// </summary>
        MobilityState ValidPosition { get; set; }

        /// <summary>
        /// The quality we're checking for
        /// </summary>
        string Quality { get; set; }

        /// <summary>
        /// The value range of the quality we're checking for
        /// </summary>
        ValueRange<int> QualityRange { get; set; }

        /// <summary>
        /// Validate the criteria against the actor and victim
        /// </summary>
        /// <param name="actor">who's doing the hitting</param>
        /// <param name="victim">who's being hit</param>
        /// <returns></returns>
        bool Validate(IMobile target, ulong distance);
    }
}
