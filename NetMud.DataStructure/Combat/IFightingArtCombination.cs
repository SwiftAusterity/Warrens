using NetMud.DataStructure.Architectural.EntityBase;
using System.Collections.Generic;

namespace NetMud.DataStructure.Combat
{
    public interface IFightingArtCombination
    {
        /// <summary>
        /// The available arts for this combo
        /// </summary>
        HashSet<IFightingArt> Arts { get; set; }

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
        bool IsValid(IEntity actor, IEntity victim);
    }
}
