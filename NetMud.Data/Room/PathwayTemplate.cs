using NetMud.Cartography;
using NetMud.Communication.Lexical;
using NetMud.Data.Architectural.DataIntegrity;
using NetMud.Data.Architectural.EntityBase;
using NetMud.Data.Linguistic;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.Room;
using NetMud.DataStructure.System;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.Room
{
    /// <summary>
    /// Backing data for pathways
    /// </summary>
    [Serializable]
    public class PathwayTemplate : EntityTemplatePartial, IPathwayTemplate
    {
        /// <summary>
        /// The system type for the entity this attaches to
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public override Type EntityClass
        {
            get { return typeof(Pathway); }
        }

        /// <summary>
        /// What type of approval is necessary for this content
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public override ContentApprovalType ApprovalType { get { return ContentApprovalType.Leader; } }

        /// <summary>
        /// keywords this entity is referrable by in the world by the parser
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public override string[] Keywords
        {
            get
            {
                if (_keywords == null || _keywords.Length == 0)
                    _keywords = new string[] { Name.ToLower() };

                return _keywords;
            }
            set { _keywords = value; }
        }

        /// <summary>
        /// DegreesFromNorth translated
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public MovementDirectionType DirectionType
        {
            get
            {
                return Utilities.TranslateToDirection(DegreesFromNorth, InclineGrade);
            }
        }

        /// <summary>
        /// 0->360 degrees with 0 being absolute north (meaning 90 is west, 180 south, etc) -1 means no cardinality
        /// </summary>
        [Range(-1, 360, ErrorMessage = "The {0} must be between {2} and {1}. -1 is for non-cardinal exits.")]
        [Display(Name = "Degrees From North", Description = "The direction on a 360 plane. 360 and 0 are both directional north. 90 is east, 180 is south, 270 is west.")]
        [DataType(DataType.Text)]
        public int DegreesFromNorth { get; set; }

        /// <summary>
        /// -100 to 100 (negative being a decline) % grade of up and down
        /// </summary>
        [Range(-100, 100, ErrorMessage = "The {0} must be between {2} and {1}. -1 is for non-cardinal exits.")]
        [Display(Name = "Incline Grade", Description = "-100 to 100 (negative being a decline) % grade of elevation change.")]
        [DataType(DataType.Text)]
        public int InclineGrade { get; set; }

        [JsonProperty("Destination")]
        private TemplateCacheKey _destination { get; set; }

        /// <summary>
        /// What is in the middle of the room
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [NonNullableDataIntegrity("Destination is invalid.")]
        [Display(Name = "To Room", Description = "The room this leads to.")]
        [DataType(DataType.Text)]
        public ILocationData Destination
        {
            get
            {
                return (ILocationData)TemplateCache.Get(_destination);
            }
            set
            {
                if (value != null)
                    _destination = new TemplateCacheKey(value);
            }
        }

        [JsonProperty("Origin")]
        private TemplateCacheKey _origin { get; set; }

        /// <summary>
        /// What is in the middle of the room
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [NonNullableDataIntegrity("Origin is invalid.")]
        [Display(Name = "From Room", Description = "The room this originates from.")]
        [DataType(DataType.Text)]
        public ILocationData Origin
        {
            get
            {
                return (ILocationData)TemplateCache.Get(_origin);
            }
            set
            {
                if (value != null)
                    _origin = new TemplateCacheKey(value);
            }
        }

        /// <summary>
        /// Framework for the physics model of an entity
        /// </summary>
        [NonNullableDataIntegrity("Physical Model is invalid.")]
        [UIHint("TwoDimensionalModel")]
        public IDimensionalModel Model { get; set; }

        /// <summary>
        /// Blank constructor
        /// </summary>
        public PathwayTemplate()
        {
            Model = new DimensionalModel();
            Descriptives = new HashSet<ISensoryEvent>();
        }

        /// <summary>
        /// Spawn new pathway with its model
        /// </summary>
        [JsonConstructor]
        public PathwayTemplate(DimensionalModel model)
        {
            Model = model;
            Descriptives = new HashSet<ISensoryEvent>();
        }

        /// <summary>
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        public override Dimensions GetModelDimensions()
        {
            return new Dimensions(Model.Height, Model.Length, Model.Width);
        }

        /// <summary>
        /// Get the live version of this in the world
        /// </summary>
        /// <returns>The live data</returns>
        public IPathway GetLiveInstance()
        {
            return LiveCache.Get<IPathway>(Id);
        }

        /// <summary>
        /// Get the significant details of what needs approval
        /// </summary>
        /// <returns>A list of strings</returns>
        public override IDictionary<string, string> SignificantDetails()
        {
            IDictionary<string, string> returnList = base.SignificantDetails();

            foreach (ISensoryEvent desc in Descriptives)
                returnList.Add("Descriptives", string.Format("{0} ({1}): {2}", desc.SensoryType, desc.Strength, desc.Event.ToString()));

            return returnList;
        }

        /// <summary>
        /// Put it in the cache
        /// </summary>
        /// <returns>success status</returns>
        public override bool PersistToCache()
        {
            try
            {
                List<IDictata> dictatas = new List<IDictata>
                {
                    new Dictata(new Lexica(LexicalType.Noun, GrammaticalType.Subject, Name))
                };
                dictatas.AddRange(Descriptives.Select(desc => desc.Event.GetDictata()));

                foreach (IDictata dictata in dictatas)
                    LexicalProcessor.VerifyDictata(dictata);

                TemplateCache.Add(this);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex, LogChannels.SystemWarnings);
                return false;
            }

            return true;
        }
    }
}
