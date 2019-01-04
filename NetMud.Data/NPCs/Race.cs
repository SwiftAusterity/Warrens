using NetMud.Data.Architectural.DataIntegrity;
using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.NPC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.NPCs
{
    /// <summary>
    /// Character race, determines loads of things
    /// </summary>
    [Serializable]
    public class Race : IRace
    {
        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Race", Description = "Name of the Race this NPC is")]
        [DataType(DataType.Text)]
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Low and High temperature range before damage starts to occur
        /// </summary>
        [Range(0, 200)]
        [Display(Name = "Heat Tolerence", Description = "The range of what temperature this can tolerate. Below this is 'too cold' and the thing will suffer ill effects.")]
        [Required]
        [UIHint("ValueRangeShort")]
        public ValueRange<short> TemperatureTolerance { get; set; }

        /// <summary>
        /// The name used to describe a large gathering of this race
        /// </summary>
        [StringDataIntegrity("Races must have a collective noun between 2 and 50 characters long.", 2, 50)]
        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Collective Noun", Description = "The word used to describe a collection of this race.")]
        [DataType(DataType.Text)]
        [Required]
        public string CollectiveNoun { get; set; }

        [Display(Name = "Aquatic", Description = "Does this live in the water?")]
        [UIHint("Boolean")]
        [Required]
        public bool Aquatic { get; set; }

        [Display(Name = "Diet", Description = "What does this npc eat?")]
        [UIHint("EnumDropDownList")]
        [Required]
        public DietType Diet { get; set; }

        [Display(Name = "Reproductive", Description = "How this NPC makes new ones of itself.")]
        [UIHint("EnumDropDownList")]
        [Required]
        public ReproductionMethod ReproductionType { get; set; }

        [Range(0, 100)]
        [Display(Name = "Spawning", Description = "How many of this does it make when it has babies.")]
        [DataType(DataType.Text)]
        [Required]
        public short SpawningCoefficient { get; set; }

        [Range(0, 100)]
        [Display(Name = "Trait Mutation", Description = "How likely is it for traits to mutate on birth.")]
        [DataType(DataType.Text)]
        [Required]
        public short TraitMutationCoefficient { get; set; }

        [Range(0, 100)]
        [Display(Name = "Trait Inheritance", Description = "How likely is it for traits to inherit on birth.")]
        [DataType(DataType.Text)]
        [Required]
        public short TraitInheritanceCoefficient { get; set; }

        [JsonProperty("ButcherResults")]
        private HashSet<TemplateCacheKey> _butcherResults { get; set; }

        [ScriptIgnore]
        [JsonIgnore]
        [Display(Name = "Butcher Results", Description = "What other objects this is made of internally when butchered")]
        [UIHint("CollectionInanimateTemplateList")]
        [InanimateCollectionTemplateDataBinder]
        public HashSet<IInanimateTemplate> ButcherResults
        {
            get
            {
                if (_butcherResults == null)
                    _butcherResults = new HashSet<TemplateCacheKey>();

                return new HashSet<IInanimateTemplate>(_butcherResults.Select(cp => TemplateCache.Get<IInanimateTemplate>(cp)));
            }
            set
            {
                if (value == null)
                    return;

                _butcherResults = new HashSet<TemplateCacheKey>(value.Where(cp => cp != null).Select(cp => new TemplateCacheKey(cp)));
            }
        }

        [JsonProperty("StomachContents")]
        private HashSet<LiveCacheKey> _stomachContents { get; set; }

        [ScriptIgnore]
        [JsonIgnore]
        public HashSet<IInanimate> StomachContents
        {
            get
            {
                if (_stomachContents == null)
                    _stomachContents = new HashSet<LiveCacheKey>();

                return new HashSet<IInanimate>(_stomachContents.Select(cp => LiveCache.Get<IInanimate>(cp)));
            }
            set
            {
                if (value == null)
                    return;

                _stomachContents = new HashSet<LiveCacheKey>(value.Select(cp => new LiveCacheKey(cp)));
            }
        }

        public int Consume(IInanimate food)
        {
            throw new NotImplementedException();
        }

        public bool Schtup(ICanReproduce mate)
        {
            throw new NotImplementedException();
        }
    }
}
