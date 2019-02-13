using NetMud.Data.Architectural.DataIntegrity;
using NetMud.Data.Architectural.PropertyBinding;
using NetMud.Data.Linguistic;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Architectural.EntityBase;
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
    /// Animal spawns
    /// </summary>
    [Serializable]
    public class Fauna : NaturalResourceDataPartial, IFauna
    {
        /// <summary>
        /// What is the % chance of generating a female instead of a male on birth
        /// </summary>
        [IntDataIntegrity("Female to male ratio must be greater than 0.", 1)]
        [Range(1, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Ratio Female to Male", Description = "The split of both how often males vs females will be spawned but also the general fertility rate of this herd.")]
        [DataType(DataType.Text)]
        public int FemaleRatio { get; set; }

        /// <summary>
        /// The absolute hard cap to natural population growth
        /// </summary>
        [IntDataIntegrity("Population Hard Cap must be greater than 0.", 1)]
        [Range(1, 2000, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Total Pop Cap", Description = "Total max pool strength this herd can get to.")]
        [DataType(DataType.Text)]
        public int PopulationHardCap { get; set; }

        [JsonProperty("Race")]
        private TemplateCacheKey _race { get; set; }

        /// <summary>
        /// What we're spawning
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        [NonNullableDataIntegrity("Race must be set.")]
        [Display(Name = "Race", Description = "What race this herd is composed of. Non-sentient races only.")]
        [UIHint("RaceList")]
        [RaceDataBinder]
        public IRace Race
        {
            get
            {
                return TemplateCache.Get<IRace>(_race);
            }
            set
            {
                _race = new TemplateCacheKey(value);
            }
        }

        public Fauna()
        {
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

            returnList.Add("Race", Race.Name);
            returnList.Add("Female Ratio", FemaleRatio.ToString());
            returnList.Add("Population Cap", PopulationHardCap.ToString());

            return returnList;
        }

        #region Rendering
        /// <summary>
        /// Render a natural resource collection to a viewer
        /// </summary>
        /// <param name="viewer">the entity looking</param>
        /// <param name="amount">How much of it there is</param>
        /// <returns>a view string</returns>
        public override ISensoryEvent RenderResourceCollection(IEntity viewer, int amount)
        {
            if (!IsVisibleTo(viewer))
            {
                return null;
            }

            var collectiveContext = new LexicalContext()
            {
                Determinant = true,
                Perspective = NarrativePerspective.SecondPerson,
                Plural = false,
                Position = LexicalPosition.Around,
                Tense = LexicalTense.Present
            };

            var discreteContext = new LexicalContext()
            {
                Determinant = true,
                Perspective = NarrativePerspective.ThirdPerson,
                Plural = false,
                Position = LexicalPosition.Attached,
                Tense = LexicalTense.Present
            };

            ISensoryEvent me = GetSelf(MessagingType.Visible);
            Lexica collectiveNoun = new Lexica(LexicalType.Noun, GrammaticalType.Descriptive, Race.CollectiveNoun, collectiveContext);
            collectiveNoun.TryModify(new Lexica(LexicalType.Adjective, GrammaticalType.Descriptive, amount.ToString(), discreteContext));
            me.Event.TryModify(collectiveNoun);

            return me;
        }
        #endregion
    }
}
