using NetMud.DataStructure.Player;
using System.Collections.Generic;

namespace NetMud.DataStructure.Combat
{
    /// <summary>
    /// Collection of fighting arts
    /// </summary>
    public interface IFightingArtCombination
    {
        /// <summary>
        /// The name of this combo
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Is this a system based combo
        /// </summary>
        bool IsSystem { get; set; }

        /// <summary>
        /// Mobile chosen fighting stance which causes FightingArtCombinations to become active or inactive
        /// </summary>
        HashSet<string> FightingStances { get; set; }

        /// <summary>
        /// The available arts for this combo
        /// </summary>
        SortedSet<IFightingArt> Arts { get; set; }

        /// <summary>
        /// Get the next move to use
        /// </summary>
        /// <param name="lastAttack">Was there an attack last go</param>
        /// <returns>The next art to use</returns>
        IFightingArt GetNext(IFightingArt lastAttack);

        /// <summary>
        /// Is this combo valid to be active at all
        /// </summary>
        /// <param name="actor">who's doing the hitting</param>
        /// <param name="victim">who's being hit</param>
        /// <returns>yea or nay</returns>
        bool IsValid(IPlayer actor, IPlayer victim, ulong distance);
    }
}
