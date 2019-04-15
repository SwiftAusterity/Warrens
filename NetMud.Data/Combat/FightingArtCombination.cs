using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Player;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.DataStructure.Combat
{
    [Serializable]
    public class FightingArtCombination : IFightingArtCombination
    {
        /// <summary>
        /// Mobile chosen fighting stance which causes FightingArtCombinations to become active or inactive
        /// </summary>
        public HashSet<string> FightingStances { get; set; }

        [JsonProperty("Arts")]
        public SortedSet<TemplateCacheKey> _arts { get; set; }

        /// <summary>
        /// The available arts for this combo
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public SortedSet<IFightingArt> Arts
        {
            get
            {
                if (_arts == null)
                {
                    _arts = new SortedSet<TemplateCacheKey>();
                }

                return new SortedSet<IFightingArt>(_arts.Select(k => TemplateCache.Get<IFightingArt>(k)));
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                _arts = new SortedSet<TemplateCacheKey>(value.Select(k => new TemplateCacheKey(k)));
            }
        }

        public FightingArtCombination()
        {
            FightingStances = new HashSet<string>();
        }

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
