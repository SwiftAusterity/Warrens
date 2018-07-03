using NetMud.Data.LookupData;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Cartography;
using Newtonsoft.Json;
using System;
using System.Web.Script.Serialization;
using NetMud.Data.DataIntegrity;
using NetMud.DataAccess.Cache;
using System.Collections.Generic;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.Behaviors.System;
using NetMud.DataStructure.Linguistic;
using NetMud.Data.ConfigData;
using NetMud.Communication.Messaging;
using NetMud.Communication.Lexicon;
using System.Linq;
using NetMud.DataAccess;

namespace NetMud.Data.EntityBackingData
{
    /// <summary>
    /// Backing data for pathways
    /// </summary>
    [Serializable]
    public class PathwayData : EntityBackingDataPartial, IPathwayData
    {
        /// <summary>
        /// The system type for the entity this attaches to
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public override Type EntityClass
        {
            get { return typeof(Game.Pathway); }
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
        /// Set of output relevant to this exit. These are essentially single word descriptions to render the path
        /// </summary>
        public HashSet<IOccurrence> Descriptives { get; set; }

        /// <summary>
        /// 0->360 degrees with 0 being absolute north (meaning 90 is west, 180 south, etc) -1 means no cardinality
        /// </summary>
        public int DegreesFromNorth { get; set; }

        /// <summary>
        /// -100 to 100 (negative being a decline) % grade of up and down
        /// </summary>
        public int InclineGrade { get; set; }

        [JsonProperty("Destination")]
        private BackingDataCacheKey _destination { get; set; }

        /// <summary>
        /// What is in the middle of the room
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [NonNullableDataIntegrity("Destination is invalid.")]
        public ILocationData Destination
        {
            get
            {
                return (ILocationData)BackingDataCache.Get(_destination);
            }
            set
            {
                if (value != null)
                    _destination = new BackingDataCacheKey(value);
            }
        }

        [JsonProperty("Origin")]
        private BackingDataCacheKey _origin { get; set; }

        /// <summary>
        /// What is in the middle of the room
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [NonNullableDataIntegrity("Origin is invalid.")]
        public ILocationData Origin
        {
            get
            {
                return (ILocationData)BackingDataCache.Get(_origin);
            }
            set
            {
                if (value != null)
                    _origin = new BackingDataCacheKey(value);
            }
        }

        /// <summary>
        /// Framework for the physics model of an entity
        /// </summary>
        [NonNullableDataIntegrity("Physical Model is invalid.")]
        public IDimensionalModel Model { get; set; }

        /// <summary>
        /// Blank constructor
        /// </summary>
        public PathwayData()
        {
        }

        /// <summary>
        /// Spawn new pathway with its model
        /// </summary>
        [JsonConstructor]
        public PathwayData(DimensionalModel model)
        {
            Model = model;
        }

        /// <summary>
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        public override Tuple<int, int, int> GetModelDimensions()
        {
            return new Tuple<int, int, int>(Model.Height, Model.Length, Model.Width);
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
            var returnList = base.SignificantDetails();

            foreach (var desc in Descriptives)
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
                var dictatas = new List<IDictata>
                {
                    new Dictata(new Lexica(LexicalType.Noun, GrammaticalType.Subject, Name))
                };
                dictatas.AddRange(Descriptives.Select(desc => desc.Event.GetDictata()));

                foreach (var dictata in dictatas)
                    LexicalProcessor.VerifyDictata(dictata);

                BackingDataCache.Add(this);
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
