using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Combat;
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
        /// Is this used situationally
        /// </summary>
        [Display(Name = "Situational Type", Description = "Is this used situationally?")]
        [UIHint("EnumDropDownList")]
        public FightingArtComboUsage SituationalUsage { get; set; }

        /// <summary>
        /// Mobile chosen fighting stance which causes FightingArtCombinations to become active or inactive
        /// </summary>
        [Display(Name = "Stances", Description = "The stances this is a part of. Free text string for use in the Stance command.")]
        [UIHint("TagContainer")]
        public HashSet<string> FightingStances { get; set; }

        [JsonProperty("Arts")]
        public HashSet<TemplateCacheKey> _arts { get; set; }

        /// <summary>
        /// The available arts for this combo
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [Display(Name = "Fighting Arts", Description = "Which fighting arts are in this combo, in order.")]
        [UIHint("FightingArtsCollection")]
        [FightingArtCollectionDataBinder]
        public HashSet<IFightingArt> Arts
        {
            get
            {
                if (_arts == null)
                {
                    _arts = new HashSet<TemplateCacheKey>();
                }

                return new HashSet<IFightingArt>(_arts.Select(k => TemplateCache.Get<IFightingArt>(k)));
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                _arts = new HashSet<TemplateCacheKey>(value.Select(k => new TemplateCacheKey(k)));
            }
        }

        public FightingArtCombination()
        {
            FightingStances = new HashSet<string>();
            SituationalUsage = FightingArtComboUsage.None;
        }

        /// <summary>
        /// Get the next move to use
        /// </summary>
        /// <param name="lastAttack">Was there an attack last go</param>
        /// <returns>The next art to use</returns>
        public IFightingArt GetNext(IFightingArt lastAttack)
        {
            var nextAttack = Arts.FirstOrDefault();
            try
            {
                if (lastAttack != null)
                {
                    if (string.IsNullOrWhiteSpace(lastAttack.RekkaKey) || lastAttack.RekkaPosition < 0)
                    {
                        var myIndex = Arts.ToList().IndexOf(lastAttack);
                        nextAttack = Arts.Skip(myIndex + 1).FirstOrDefault();
                    }
                    else
                    {
                        nextAttack = Arts.FirstOrDefault(art => !string.IsNullOrWhiteSpace(art.RekkaKey) 
                                                                && art.RekkaKey.Equals(lastAttack.RekkaKey) 
                                                                && art.RekkaPosition == lastAttack.RekkaPosition + 1);
                    }

                    if (nextAttack == null)
                    {
                        //just throwing to redefault
                        throw new Exception();
                    }
                }
            }
            catch(Exception ex)
            {
                LoggingUtility.LogError(ex);
                nextAttack = Arts.FirstOrDefault();
            }

            return nextAttack;
        }

        /// <summary>
        /// Is this combo valid to be active at all
        /// </summary>
        /// <param name="actor">who's doing the hitting</param>
        /// <param name="victim">who's being hit</param>
        /// <returns>yea or nay</returns>
        public bool IsValid(IMobile actor, IMobile victim, ulong distance)
        {
            return FightingStances.Count == 0 || FightingStances.Contains(actor.Stance);
        }
    }
}
