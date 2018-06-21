using NetMud.Cartography;
using NetMud.Data.DataIntegrity;
using NetMud.Data.Game;
using NetMud.Data.System;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Behaviors.System;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.EntityBackingData
{
    /// <summary>
    /// Backing data for locales
    /// </summary>
    public class LocaleData : EntityBackingDataPartial, ILocaleData
    {
        [JsonIgnore]
        [ScriptIgnore]
        public override Type EntityClass
        {
            get { return typeof(Locale); }
        }

        /// <summary>
        /// What type of approval is necessary for this content
        /// </summary>
        public override ContentApprovalType ApprovalType { get { return ContentApprovalType.Staff; } }

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
        /// Is this zone discoverable?
        /// </summary>
        public bool AlwaysDiscovered { get; set; }

        /// <summary>
        /// When this locale dies off
        /// </summary>
        public DateTime RollingExpiration { get; set; }

        /// <summary>
        /// The interior map of the locale
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public IMap Interior { get; set; }

        [JsonProperty("ParentLocation")]
        private BackingDataCacheKey _parentLocation { get; set; }

        /// <summary>
        /// The zone this belongs to
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [NonNullableDataIntegrity("Locales must have a zone affiliation.")]
        public IZoneData ParentLocation
        {
            get
            {
                return BackingDataCache.Get<IZoneData>(_parentLocation);
            }
            set
            {
                if (value != null)
                    _parentLocation = new BackingDataCacheKey(value);
            }
        }

        public LocaleData()
        {
        }

        /// <summary>
        ///List of rooms
        /// </summary>
        public IEnumerable<IRoomData> Rooms()
        {
            return BackingDataCache.GetAll<IRoomData>().Where(room => room.ParentLocation.Equals(this));
        }

        /// <summary>
        /// Regenerate the internal map for the locale; try not to do this often
        /// </summary>
        public void RemapInterior()
        {
            var remainingRooms = Rooms();
            var returnMap = Cartographer.GenerateMapFromRoom(CentralRoom(), remainingRooms.Count() / 2, new HashSet<IRoomData>(remainingRooms), true);

            Interior = new Map(returnMap, false);
        }

        /// <summary>
        /// Get the central room for a Z plane
        /// </summary>
        /// <param name="zIndex">The Z plane to get the central room of</param>
        /// <returns>The room that is in the center of the Z plane</returns>
        public IRoomData CentralRoom(int zIndex = -1)
        {
            var roomsPlane = Rooms().Where(room => zIndex == -1 || (room.Coordinates != null && room.Coordinates.Item3 == zIndex));

            //TODO 
            return roomsPlane.FirstOrDefault();
        }

        /// <summary>
        /// Mean diameter of this locale by room dimensions
        /// </summary>
        /// <returns>H,W,D</returns>
        public Tuple<int, int, int> Diameter()
        {
            return new Tuple<int, int, int>(0, 0, 0);
        }

        /// <summary>
        /// Dimensional size at max for h,w,d
        /// </summary>
        /// <returns>H,W,D</returns>
        public Tuple<int, int, int> FullDimensions()
        {
            return new Tuple<int, int, int>(0, 0, 0);
        }

        /// <summary>
        /// Get the dimensions of this model, passthru to FullDimensions
        /// </summary>
        /// <returns></returns>
        public override Tuple<int, int, int> GetModelDimensions()
        {
            return FullDimensions();
        }

        /// <summary>
        /// Render the interior map
        /// </summary>
        /// <param name="zIndex">Z Plane this is getting a map for</param>
        /// <param name="forAdmin">ignore visibility</param>
        /// <returns>The flattened map</returns>
        public string RenderMap(int zIndex, bool forAdmin = false)
        {
            return Rendering.RenderRadiusMap(this, 10, zIndex, forAdmin).Item2;
        }

        /// <summary>
        /// Get the live version of this in the world
        /// </summary>
        /// <returns>The live data</returns>
        public ILocale GetLiveInstance()
        {
            return LiveCache.Get<ILocale>(Id);
        }
    }
}
