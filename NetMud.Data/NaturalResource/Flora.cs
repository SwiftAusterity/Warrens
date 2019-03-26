using NetMud.Communication.Lexical;
using NetMud.Communication.Messaging;
using NetMud.Data.Architectural.DataIntegrity;
using NetMud.Data.Architectural.PropertyBinding;
using NetMud.Data.Linguistic;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.NaturalResource;
using NetMud.DataStructure.System;
using NetMud.DataStructure.Zone;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Script.Serialization;

namespace NetMud.Data.NaturalResource
{
    /// <summary>
    /// Plants, all elements can be nullable (one has to exist)
    /// </summary>
    [Serializable]
    public class Flora : NaturalResourceDataPartial, IFlora
    {
        /// <summary>
        /// How much sunlight does this need to spawn
        /// </summary>
        [Display(Name = "Sunlight", Description = "How much sunlight does this need to spawn.")]
        [DataType(DataType.Text)]
        public int SunlightPreference { get; set; }

        /// <summary>
        /// Does this plant go dormant in colder weather
        /// </summary>
        [Display(Name = "Coniferous", Description = "Does this continue to grow in the winter.")]
        [UIHint("Boolean")]
        public bool Coniferous { get; set; }

        [JsonProperty("Wood")]
        private TemplateCacheKey _wood { get; set; }

        /// <summary>
        /// Bulk material of plant. Stem, trunk, etc.
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        [NonNullableDataIntegrity("Wood must have a value.")]
        [Display(Name = "Wood/Bark", Description = "Bulk material of plant. Stem, trunk, etc.")]
        [UIHint("MaterialList")]
        [MaterialDataBinder]
        public IMaterial Wood
        { 
            get
            {
                return TemplateCache.Get<IMaterial>(_wood);
            }
            set
            {
                _wood = new TemplateCacheKey(value);
            }
        }

        [JsonProperty("Flower")]
        private TemplateCacheKey _flower { get; set; }

        /// <summary>
        /// Flowering element of plant
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        [Display(Name = "Flower", Description = "Flowering element of plant")]
        [UIHint("InanimateTemplateList")]
        [InanimateTemplateDataBinder]
        public IInanimateTemplate Flower
        { 
            get
            {
                return TemplateCache.Get<IInanimateTemplate>(_flower);
            }
            set
            {
                _flower = new TemplateCacheKey(value);
            }
        }

        [JsonProperty("Leaf")]
        private TemplateCacheKey _leaf { get; set; }

        /// <summary>
        /// Leaves of the plant.
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        [Display(Name = "Leaves", Description = "Leaves of the plant.")]
        [UIHint("InanimateTemplateList")]
        [InanimateTemplateDataBinder]
        public IInanimateTemplate Leaf 
        { 
            get
            {
                return TemplateCache.Get<IInanimateTemplate>(_leaf);
            }
            set
            {
                _leaf = new TemplateCacheKey(value);
            }
        }

        [JsonProperty("Fruit")]
        private TemplateCacheKey _fruit { get; set; }

        /// <summary>
        /// Fruit of the plant, can be inedible like a pinecone
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        [Display(Name = "Fruit", Description = "Fruit of the plant, can be inedible like a pinecone")]
        [UIHint("InanimateTemplateList")]
        [InanimateTemplateDataBinder]
        public IInanimateTemplate Fruit
        { 
            get
            {
                return TemplateCache.Get<IInanimateTemplate>(_fruit);
            }
            set
            {
                _fruit = new TemplateCacheKey(value);
            }
        }

        [JsonProperty("Seed")]
        private TemplateCacheKey _seed { get; set; }

        /// <summary>
        /// Seed of the plant.
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        [Display(Name = "Seed", Description = "Seed of the plant.")]
        [UIHint("InanimateTemplateList")]
        [InanimateTemplateDataBinder]
        public IInanimateTemplate Seed 
        { 
            get
            {
                return TemplateCache.Get<IInanimateTemplate>(_seed);
            }
            set
            {
                _seed = new TemplateCacheKey(value);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Flora()
        {
            OccursIn = new HashSet<Biome>();
            ElevationRange = new ValueRange<int>();
            TemperatureRange = new ValueRange<int>();
            HumidityRange = new ValueRange<int>();
        }

        /// <summary>
        /// Gets the errors for data fitness
        /// </summary>
        /// <returns>a bunch of text saying how awful your data is</returns>
        public override IList<string> FitnessReport()
        {
            IList<string> dataProblems = base.FitnessReport();

            if (Flower == null && Seed == null && Leaf == null && Fruit == null)
            {
                dataProblems.Add("At least one part of this plant must have a value.");
            }

            return dataProblems;
        }

        /// <summary>
        /// Get the significant details of what needs approval
        /// </summary>
        /// <returns>A list of strings</returns>
        public override IDictionary<string, string> SignificantDetails()
        {
            IDictionary<string, string> returnList = base.SignificantDetails();

            returnList.Add("Sunlight", SunlightPreference.ToString());
            returnList.Add("Coniferous", Coniferous.ToString());
            returnList.Add("Wood", Wood.Name);
            returnList.Add("Flower", Flower.Name);
            returnList.Add("Leaf", Leaf.Name);
            returnList.Add("Fruit", Fruit.Name);
            returnList.Add("Seed", Seed.Name);

            return returnList;
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

            var collectiveContext = new LexicalContext(viewer)
            {
                Determinant = false,
                Perspective = NarrativePerspective.SecondPerson,
                Plural = false,
                Position = LexicalPosition.None,
                Tense = LexicalTense.Present
            };

            var discreteContext = new LexicalContext(viewer)
            {
                Determinant = false,
                Perspective = NarrativePerspective.ThirdPerson,
                Plural = false,
                Position = LexicalPosition.Attached,
                Tense = LexicalTense.Present
            };

            var sizeWord = "large";
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
                sizeWord = "vast";
            }

            var observer = new SensoryEvent(new Lexica(LexicalType.Pronoun, GrammaticalType.Subject, "you", collectiveContext), 0, MessagingType.Visible)
            {
                Strength = GetVisibleDelta(viewer)
            };

            var collectiveNoun = new SensoryEvent(new Lexica(LexicalType.Noun, GrammaticalType.DirectObject, "forest", collectiveContext),
                                                GetVisibleDelta(viewer), MessagingType.Visible);

            var me = GetSelf(MessagingType.Visible, GetVisibleDelta(viewer));
            me.Event.Role = GrammaticalType.Descriptive;

            collectiveNoun.TryModify(me);

            var senseVerb = new SensoryEvent(new Lexica(LexicalType.Verb, GrammaticalType.Verb, "see", collectiveContext), me.Strength, MessagingType.Visible);

            if (!string.IsNullOrWhiteSpace(sizeWord))
            {
                collectiveNoun.TryModify(new Lexica(LexicalType.Adjective, GrammaticalType.Descriptive, sizeWord, discreteContext));
            }

            senseVerb.TryModify(collectiveNoun);
            observer.TryModify(senseVerb);

            return new LexicalParagraph(observer);
        }
        #endregion
    }
}
