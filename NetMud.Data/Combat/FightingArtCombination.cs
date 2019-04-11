using NetMud.DataStructure.Player;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.DataStructure.Combat
{
    [Serializable]
    public class FightingArtCombination : IFightingArtCombination
    {
        /// <summary>
        /// The available arts for this combo
        /// </summary>
        public HashSet<IFightingArt> Arts { get; set; }

        /// <summary>
        /// Get the next move to use
        /// </summary>
        /// <param name="lastAttack">Was there an attack last go</param>
        /// <returns>The next art to use</returns>
        public IFightingArt GetNext(IFightingArt lastAttack)
        {
            return lastAttack;
        }

        /// <summary>
        /// Is this combo valid to be active at all
        /// </summary>
        /// <param name="actor">who's doing the hitting</param>
        /// <param name="victim">who's being hit</param>
        /// <returns>yea or nay</returns>
        public bool IsValid(IPlayer actor, IPlayer victim, ulong distance)
        {
            return Arts.All(art => art.IsValid(actor, victim, distance));
        }
    }
}
