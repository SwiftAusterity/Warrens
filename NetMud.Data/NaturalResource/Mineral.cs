using NetMud.Communication.Lexical;
using NetMud.Communication.Messaging;
using NetMud.Data.Architectural.DataIntegrity;
using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.NaturalResource;
using NetMud.DataStructure.System;
using NetMud.DataStructure.Zone;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace NetMud.Data.NaturalResource
{
    /// <summary>
    /// Rocks, minable metals and dirt
    /// </summary>
    [Serializable]
    public class Mineral : NaturalResourceDataPartial, IMineral
    {
        /// <summary>
        /// How soluble the dirt is
        /// </summary>
        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Solubility", Description = "The factor of how well this dissolves in water.")]
        [DataType(DataType.Text)]
        public int Solubility { get; set; }

        /// <summary>
        /// How fertile the dirt generally is
        /// </summary>
        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Dirt Fertility", Description = "How likely are fauna to grow in this if it is used as dirt.")]
        [DataType(DataType.Text)]
        public int Fertility { get; set; }

        [JsonProperty("Rock")]
        private TemplateCacheKey _rock { get; set; }

        /// <summary>
        /// What is the solid, crystallized form of this
        /// </summary>
        [JsonIgnore]

        [NonNullableDataIntegrity("Rock must have a value.")]
        [Display(Name = "Rock", Description = "What object is used to refer to this in rock form.")]
        [UIHint("MaterialList")]
        [MaterialDataBinder]
        public IMaterial Rock
        {
            get
            {
                return TemplateCache.Get<IMaterial>(_rock);
            }
            set
            {
                _rock = new TemplateCacheKey(value);
            }
        }

        [JsonProperty("Dirt")]
        private TemplateCacheKey _dirt { get; set; }

        /// <summary>
        /// What is the scattered, ground form of this
        /// </summary>
        [JsonIgnore]

        [NonNullableDataIntegrity("Dirt must have a value.")]
        [Display(Name = "Dirt", Description = "What object is used to refer to this in dirt form.")]
        [UIHint("MaterialList")]
        [MaterialDataBinder]
        public IMaterial Dirt
        {
            get
            {
                return TemplateCache.Get<IMaterial>(_dirt);
            }
            set
            {
                _dirt = new TemplateCacheKey(value);
            }
        }

        [JsonProperty("Ores")]
        private HashSet<TemplateCacheKey> _ores { get; set; }

        /// <summary>
        /// What medium minerals this can spawn in
        /// </summary>
        [JsonIgnore]

        [Display(Name = "Ores", Description = "What ores this contains when mined as rock.")]
        [UIHint("CollectionMineralList")]
        [MineralCollectionDataBinder]
        public HashSet<IMineral> Ores
        {
            get
            {
                if (_ores == null)
                {
                    _ores = new HashSet<TemplateCacheKey>();
                }

                return new HashSet<IMineral>(TemplateCache.GetMany<IMineral>(_ores));
            }
            set
            {
                _ores = new HashSet<TemplateCacheKey>(value.Select(m => new TemplateCacheKey(m)));
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Mineral()
        {
            Ores = new HashSet<IMineral>();
            OccursIn = new HashSet<Biome>();
            ElevationRange = new ValueRange<int>();
            TemperatureRange = new ValueRange<int>();
            HumidityRange = new ValueRange<int>();
        }

        /// <summary>
        /// Get the significant details of what needs approval
        /// </summary>
        /// <returns>A list of strings</returns>
        public override IDictionary<string, string> SignificantDetails()
        {
            IDictionary<string, string> returnList = base.SignificantDetails();

            returnList.Add("Solubility", Solubility.ToString());
            returnList.Add("Fertility", Fertility.ToString());
            returnList.Add("Rock", Rock.Name);
            returnList.Add("Dirt", Dirt.ToString());

            foreach(IMineral ore in Ores)
            {
                returnList.Add("Ore", ore.Name);
            }

            return returnList;
        }

        public override bool CanSpawnIn(IGlobalPosition location)
        {
            bool returnValue = true;

            return base.CanSpawnIn(location) && returnValue;
        }

        public override bool ShouldSpawnIn(IGlobalPosition location)
        {
            bool returnValue = true;

            return base.ShouldSpawnIn(location) && returnValue;
        }

        #region Rendering
        /// <summary>
        /// Render a natural resource collection to a viewer
        /// </summary>
        /// <param name="viewer">the entity looking</param>
        /// <param name="amount">How much of it there is</param>
        /// <returns>a view string</returns>
        public override ILexicalParagraph RenderResourceCollection(IEntity viewer, int amount)
        {
            if (amount <= 0)
            {
                return new LexicalParagraph();
            }

            LexicalContext personalContext = new(viewer)
            {
                Determinant = false,
                Perspective = NarrativePerspective.SecondPerson,
                Plural = false,
                Position = LexicalPosition.None,
                Tense = LexicalTense.Present
            };

            LexicalContext discreteContext = new(viewer)
            {
                Determinant = false,
                Perspective = NarrativePerspective.ThirdPerson,
                Plural = false,
                Position = LexicalPosition.Attached,
                Tense = LexicalTense.Present
            };

            LexicalContext collectiveContext = new(viewer)
            {
                Determinant = false,
                Perspective = NarrativePerspective.ThirdPerson,
                Plural = false,
                Position = LexicalPosition.PartOf,
                Tense = LexicalTense.Present
            };
            string sizeWord;
            if (amount < 20)
            {
                sizeWord = "sparse";
            }
            else if (amount < 50)
            {
                sizeWord = "small";
            }
            else if (amount < 200)
            {
                sizeWord = "";
            }
            else
            {
                sizeWord = "enormous";
            }

            SensoryEvent observer = new(new Linguistic.Lexica(LexicalType.Pronoun, GrammaticalType.Subject, "you", personalContext), 0, MessagingType.Visible)
            {
                Strength = GetVisibleDelta(viewer)
            };

            SensoryEvent collectiveNoun = new(new Linguistic.Lexica(LexicalType.Noun, GrammaticalType.DirectObject, "outcropping", personalContext),
                                                GetVisibleDelta(viewer), MessagingType.Visible);

            ISensoryEvent me = GetSelf(MessagingType.Visible, GetVisibleDelta(viewer));
            me.Event.Role = GrammaticalType.IndirectObject;
            me.Event.Context = collectiveContext;

            collectiveNoun.TryModify(me);

            SensoryEvent senseVerb = new(new Linguistic.Lexica(LexicalType.Verb, GrammaticalType.Verb, "see", personalContext), me.Strength, MessagingType.Visible);

            if (!string.IsNullOrWhiteSpace(sizeWord))
            {
                collectiveNoun.TryModify(new Linguistic.Lexica(LexicalType.Adjective, GrammaticalType.Descriptive, sizeWord, discreteContext));
            }

            senseVerb.TryModify(collectiveNoun);
            observer.TryModify(senseVerb);

            return new LexicalParagraph(observer);
        }
        #endregion
    }
}
