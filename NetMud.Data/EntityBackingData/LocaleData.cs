using NetMud.DataStructure.Base.Place;
using System;
using System.Collections.Generic;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.Data.Game;
using Newtonsoft.Json;
using System.Web.Script.Serialization;
using System.Linq;
using NetMud.DataAccess.Cache;
using NetMud.Data.DataIntegrity;

namespace NetMud.Data.EntityBackingData
{
    /// <summary>
    /// Backing data for locales
    /// </summary>
    public class LocaleData : EntityBackingDataPartial, ILocaleData
    {
        public override Type EntityClass
        {
            get { return typeof(Locale); }
        }

        /// <summary>
        /// If this is discoverable or not
        /// </summary>
        public bool AlwaysVisible { get; set; }

        /// <summary>
        /// The interior map of the locale
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public IMap Interior { get; set; }

        [JsonProperty("Affiliation")]
        private long _affiliation { get; set; }

        /// <summary>
        /// The zone this belongs to
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [NonNullableDataIntegrity("Locales must have a zone affiliation.")]
        public IZoneData Affiliation
        {
            get
            {
                return BackingDataCache.Get<IZoneData>(_affiliation);
            }
            set
            {
                if (value != null)
                    _affiliation = value.ID;
            }
        }

        [JsonProperty("Rooms")]
        private HashSet<long> _rooms { get; set; }

        /// <summary>
        ///List of perm locales
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public HashSet<IRoomData> Rooms
        {
            get
            {
                if (_rooms != null)
                    return new HashSet<IRoomData>(BackingDataCache.GetMany<IRoomData>(_rooms));

                return null;
            }
            set
            {
                if (value == null)
                    return;

                _rooms = new HashSet<long>(value.Select(k => k.ID));
            }
        }

        [JsonProperty("ZoneExits")]
        private IEnumerable<long> _zoneExits { get; set; }

        /// <summary>
        ///List of perm locales
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IEnumerable<IZoneData> ZoneExits
        {
            get
            {
                if (_zoneExits != null)
                    return BackingDataCache.GetMany<IZoneData>(_zoneExits);

                return null;
            }
            set
            {
                if (value == null)
                    return;

                _zoneExits = value.Select(k => k.ID);
            }
        }

        [JsonProperty("LocaleExits")]
        private IEnumerable<long> _localeExits { get; set; }

        /// <summary>
        ///List of perm locales
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IEnumerable<ILocaleData> LocaleExits
        {
            get
            {
                if (_localeExits != null)
                    return BackingDataCache.GetMany<ILocaleData>(_localeExits);

                return null;
            }
            set
            {
                if (value == null)
                    return;

                _localeExits = value.Select(k => k.ID);
            }
        }

        /// <summary>
        /// Get the central room for a Z plane
        /// </summary>
        /// <param name="zIndex">The Z plane to get the central room of</param>
        /// <returns>The room that is in the center of the Z plane</returns>
        public IRoomData CentralRoom(int zIndex = -1)
        {
            return null;
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
            return string.Empty;
        }
    }
}
