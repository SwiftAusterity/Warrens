using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.World;
using NetMud.DataStructure.Behaviors.System;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.EntityBackingData
{
    /// <summary>
    /// Backing data for IGaia, configuration settings for each zone-cluster
    /// </summary>
    [Serializable]
    public class GaiaData : EntityBackingDataPartial, IGaiaData
    {
        /// <summary>
        /// The system type of data this attaches to
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override Type EntityClass
        {
            get { return typeof(Game.Gaia); }
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

        [JsonProperty("CelestialBodies")]
        public IEnumerable<BackingDataCacheKey> _celestialBodies { get; set; }

        /// <summary>
        /// Celestial bodies for this world
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public IEnumerable<ICelestial> CelestialBodies
        {
            get
            {
                if (_celestialBodies == null)
                    _celestialBodies = Enumerable.Empty<BackingDataCacheKey>();

                return _celestialBodies.Select(cp => BackingDataCache.Get<ICelestial>(cp));
            }
            set
            {
                if (value == null)
                    return;

                _celestialBodies = value.Select(cp => new BackingDataCacheKey(cp));
            }
        }

        /// <summary>
        /// Time keeping for this world
        /// </summary>
        public IChronology ChronologicalSystem { get; set; }

        public IGaia GetLiveInstance()
        {
            return LiveCache.Get<IGaia>(Id, GetType());
        }

        /// <summary>
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        public override Tuple<int, int, int> GetModelDimensions()
        {
            return new Tuple<int, int, int>(1, 1, 1);
        }

        /// <summary>
        /// Get the zones associated with this world
        /// </summary>
        /// <returns>list of zones</returns>
        public IEnumerable<IZoneData> GetZones()
        {
            return BackingDataCache.GetAll<IZoneData>().Where(zone => zone.World.Equals(this));
        }

        /// <summary>
        /// Get the significant details of what needs approval
        /// </summary>
        /// <returns>A list of strings</returns>
        public override IDictionary<string, string> SignificantDetails()
        {
            var returnList = base.SignificantDetails();

            return returnList;
        }
    }
}
