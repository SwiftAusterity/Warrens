using NetMud.DataStructure.Combat;

namespace NetMud.DataStructure.Architectural.ActorBase
{
    public interface ICanFight
    {
        /// <summary>
        /// How much stagger this currently has
        /// </summary>
        int Stagger { get; set; }

        /// <summary>
        /// How much stagger resistance this currently has
        /// </summary>
        int Sturdy { get; set; }

        /// <summary>
        /// How off balance this is. Positive is forward leaning, negative is backward leaning, 0 is in balance
        /// </summary>
        int Balance { get; set; }

        /// <summary>
        /// What stance this is currently in (for fighting art combo choosing)
        /// </summary>
        string Stance { get; set; }

        /// <summary>
        /// Is the current attack executing
        /// </summary>
        bool Executing { get; set; }

        /// <summary>
        /// Last attack executed
        /// </summary>
        IFightingArt LastAttack { get; set; }

        /// <summary>
        /// Last combo used for attacking
        /// </summary>
        IFightingArtCombination LastCombo { get; set; }
    }
}
