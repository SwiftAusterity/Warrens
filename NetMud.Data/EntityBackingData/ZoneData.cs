using NetMud.Communication.Lexicon;
using NetMud.Communication.Messaging;
using NetMud.Data.ConfigData;
using NetMud.Data.Game;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.Behaviors.System;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.SupportingClasses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.EntityBackingData
{
    public class ZoneData : LocationDataEntityPartial, IZoneData
    {
        /// <summary>
        /// The system type of data this attaches to
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override Type EntityClass
        {
            get { return typeof(Zone); }
        }

        /// <summary>
        /// What type of approval is necessary for this content
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public override ContentApprovalType ApprovalType { get { return ContentApprovalType.Admin; } }

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
        /// Base elevation used in generating locales
        /// </summary>
        public int BaseElevation { get; set; }

        /// <summary>
        /// Temperature variance for generating locales
        /// </summary>
        public int TemperatureCoefficient { get; set; }

        /// <summary>
        /// Barometric variance for generating locales
        /// </summary>
        public int PressureCoefficient { get; set; }

        /// <summary>
        /// What the natural biome is for generating locales
        /// </summary>
        public Biome BaseBiome { get; set; }

        /// <summary>
        /// Is this zone discoverable?
        /// </summary>
        public bool AlwaysDiscovered { get; set; }

        /// <summary>
        /// Set of output relevant to this exit. These are essentially single word descriptions to render the path
        /// </summary>
        public HashSet<IOccurrence> Descriptives { get; set; }

        [JsonProperty("Templates")]
        private HashSet<BackingDataCacheKey> _templates { get; set; }

        /// <summary>
        /// Adventure templates valid for this zone
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public HashSet<IAdventureTemplate> Templates
        {
            get
            {
                if (_templates != null)
                    return new HashSet<IAdventureTemplate>(BackingDataCache.GetMany<IAdventureTemplate>(_templates));

                return null;
            }
            set
            {
                if (value == null)
                    return;

                _templates = new HashSet<BackingDataCacheKey>(value.Select(k => new BackingDataCacheKey(k)));
            }
        }

        [JsonProperty("NaturalResourceSpawn")]
        private IDictionary<BackingDataCacheKey, int> _naturalResourceSpawn { get; set; }

        /// <summary>
        /// Collection of model section name to material composition mappings
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IDictionary<INaturalResource, int> NaturalResourceSpawn
        {
            get
            {
                if (_naturalResourceSpawn != null)
                    return _naturalResourceSpawn.ToDictionary(k => BackingDataCache.Get<INaturalResource>(k.Key), k => k.Value);

                return null;
            }
            set
            {
                if (value == null)
                    return;

                _naturalResourceSpawn = value.ToDictionary(k => new BackingDataCacheKey(k.Key), k => k.Value);
            }
        }

        /// <summary>
        /// Blank constructor
        /// </summary>
        public ZoneData()
        {
            Templates = new HashSet<IAdventureTemplate>();
            NaturalResourceSpawn = new Dictionary<INaturalResource, int>();
        }

        /// <summary>
        /// Get the total rough dimensions of the zone
        /// </summary>
        /// <returns></returns>
        public override Tuple<int, int, int> GetModelDimensions()
        {
            //TODO
            return new Tuple<int, int, int>(1, 1, 1);
        }

        /// <summary>
        /// Get the live version of this in the world
        /// </summary>
        /// <returns>The live data</returns>
        public override ILocation GetLiveInstance()
        {
            return LiveCache.Get<IZone>(Id);
        }

        /// <summary>
        /// Get the significant details of what needs approval
        /// </summary>
        /// <returns>A list of strings</returns>
        public override IDictionary<string, string> SignificantDetails()
        {
            var returnList = base.SignificantDetails();

            returnList.Add("Elevation", BaseElevation.ToString());
            returnList.Add("Temperature", TemperatureCoefficient.ToString());
            returnList.Add("Pressure", PressureCoefficient.ToString());
            returnList.Add("Biome", BaseBiome.ToString());
            returnList.Add("Always Discovered", AlwaysDiscovered.ToString());

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

                foreach(var dictata in dictatas)
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
