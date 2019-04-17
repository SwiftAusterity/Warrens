using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Combat;
using NetMud.DataStructure.Player;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.Combat
{
    [Serializable]
    public class FightingArtCombination : IFightingArtCombination
    {
        /// <summary>
        /// The name of this combo
        /// </summary>
        [Display(Name = "Name", Description = "The name to reference this by.")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        /// <summary>
        /// Is this a system based combo
        /// </summary>
        [Display(Name = "System", Description = "Is this a system combination. (so NPCs can use)")]
        [UIHint("Boolean")]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Mobile chosen fighting stance which causes FightingArtCombinations to become active or inactive
        /// </summary>
        [Display(Name = "Stances", Description = "The stances this is a part of. Free text string for use in the Stance command.")]
        [UIHint("TagContainer")]
        public HashSet<string> FightingStances { get; set; }

        [JsonProperty("Arts")]
        public SortedSet<TemplateCacheKey> _arts { get; set; }

        /// <summary>
        /// The available arts for this combo
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [Display(Name = "Fighting Arts", Description = "Which fighting arts are in this combo, in order.")]
        [UIHint("FightingArtsCollection")]
        [FightingArtCollectionDataBinder]
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
