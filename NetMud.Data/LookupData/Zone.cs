using NetMud.Data.System;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace NetMud.Data.LookupData
{
    /// <summary>
    /// Zones contain rooms
    /// </summary>
    [Serializable]
    public class Zone : LookupDataPartial, IZone
    {
        /// <summary>
        /// The midline elevation point "sea level" for this zone
        /// </summary>
        public int BaseElevation { get; set; }

        /// <summary>
        /// The fudge value for temperature variance
        /// </summary>
        public int TemperatureCoefficient { get; set; }

        /// <summary>
        /// The fudge value for pressure (weather pattern) variance
        /// </summary>
        public int PressureCoefficient { get; set; }

        /// <summary>
        /// Is this zone ownership malleable
        /// </summary>
        public bool Claimable { get; set; }

        /// <summary>
        /// The name it will confer to the world it loads to if it is the first zone to load a world
        /// </summary>
        public string WorldName { get; set; }

        [ScriptIgnore]
        [JsonIgnore]
        private long _worldId { get; set; }

        /// <summary>
        /// What world does this belong to (determined after load)
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IWorld World
        {
            get
            {
                if (_worldId > -1)
                    return BackingDataCache.Get<IWorld>(_worldId);

                return null;
            }
            set
            {
                _worldId = value.ID;
            }
        }

        [ScriptIgnore]
        [JsonIgnore]
        private IMap _zoneMap { get; set; }

        /// <summary>
        /// The room array that makes up the world
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IMap ZoneMap
        {
            get
            {
                if (_zoneMap == null && World != null)
                    _zoneMap = new Map(Cartography.Cartographer.GetZoneMap(World.WorldMap.CoordinatePlane, ID), true);

                return _zoneMap;
            }
        }

        /// <summary>
        /// New up a "blank" zone entry
        /// </summary>
        public Zone()
        {
            ID = -1;
            Created = DateTime.UtcNow;
            LastRevised = DateTime.UtcNow;
            Name = "NotImpl";
            WorldName = "Zero";

            BaseElevation = 0;
            TemperatureCoefficient = 0;
            PressureCoefficient = 0;
            Claimable = false;
        }

        /// <summary>
        /// Renders the help text for this data object
        /// </summary>
        /// <returns>help text</returns>
        public override IEnumerable<string> RenderHelpBody()
        {
            return base.RenderHelpBody();
        }

        /// <summary>
        /// Getall the rooms for the zone
        /// </summary>
        /// <returns>the rooms for the zone</returns>
        public IEnumerable<IRoomData> Rooms()
        {
            return null;
        }

        /// <summary>
        /// Get the absolute center room of the zone
        /// </summary>
        /// <returns>the central room of the zone</returns>
        public IRoomData CentralRoom()
        {
            return null;
        }

        /// <summary>
        /// Get the basic map render for the zone
        /// </summary>
        /// <returns>the zone map in ascii</returns>
        public string RenderMap()
        {
            return String.Empty;
        }

        /// <summary>
        /// Gets the ascii render of all the rooms
        /// </summary>
        /// <returns></returns>
        public string RenderRoomMap()
        {
            return String.Empty;
        }

        /// <summary>
        /// The diameter of the zone
        /// </summary>
        /// <returns>the diameter of the zone in room count</returns>
        public int Diameter()
        {
            return -1;
        }

        /// <summary>
        /// Calculate the theoretical dimensions of the zone in inches
        /// </summary>
        /// <returns>height, width, depth</returns>
        public Tuple<int, int, int> FullDimensions()
        {
            int height = -1, width = -1, depth = -1;

            return new Tuple<int, int, int>(height, width, depth);
        }
    }
}
