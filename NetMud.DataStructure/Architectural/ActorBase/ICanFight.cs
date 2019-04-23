using NetMud.DataStructure.Combat;
using NetMud.DataStructure.Player;
using System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Architectural.ActorBase
{
    /// <summary>
    /// Needed for any entities that can fight
    /// </summary>
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
        /// Who you're primarily attacking
        /// </summary>
        IPlayer PrimaryTarget { get; set; }

        /// <summary>
        /// Who you're fighting in general
        /// </summary>
        HashSet<Tuple<IPlayer, int>> EnemyGroup { get; set; }

        /// <summary>
        /// Who you're support/tank focus is
        /// </summary>
        IPlayer PrimaryDefending { get; set; }

        /// <summary>
        /// Who is in your group
        /// </summary>
        HashSet<IPlayer> AllianceGroup { get; set; }

        /// <summary>
        /// Get the target to attack
        /// </summary>
        /// <returns>A target or self if shadowboxing</returns>
        IPlayer GetTarget();

        /// <summary>
        /// Is this actor in combat
        /// </summary>
        /// <returns>yes or no</returns>
        bool IsFighting();

        /// <summary>
        /// Stop all aggression
        /// </summary>
        void StopFighting();

        /// <summary>
        /// Start a fight or switch targets forcibly
        /// </summary>
        /// <param name="victim"></param>
        void StartFighting(IPlayer victim);

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
