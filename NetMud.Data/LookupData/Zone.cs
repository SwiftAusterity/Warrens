using NetMud.Data.System;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using NetMud.DataStructure.Base.System;

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
        /// Gets the errors for data fitness
        /// </summary>
        /// <returns>a bunch of text saying how awful your data is</returns>
        public override IList<string> FitnessReport()
        {
            var dataProblems = base.FitnessReport();

            if (String.IsNullOrWhiteSpace(WorldName))
                dataProblems.Add("World name is empty or invalid.");

            if (World == null)
                dataProblems.Add("World is invalid.");

            if (ZoneMap == null)
                dataProblems.Add("Zone map is invalid.");

            if (!Rooms().Any() || Rooms().Any(r => r == null))
                dataProblems.Add("Zone has no rooms or at least one invalid room.");

            return dataProblems;
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
            return BackingDataCache.GetAll<IRoomData>().Where(room => room.ZoneAffiliation.Equals(this));
        }

        /// <summary>
        /// Get the absolute center room of the zone
        /// </summary>
        /// <returns>the central room of the zone</returns>
        public IRoomData CentralRoom(int zIndex = -1)
        {
            return Cartography.Cartographer.FindCenterOfMap(ZoneMap.CoordinatePlane, zIndex);
        }

        /// <summary>
        /// Get the basic map render for the zone
        /// </summary>
        /// <returns>the zone map in ascii</returns>
        public string RenderMap(int zIndex, bool forAdmin = false)
        {
            return Cartography.Rendering.RenderMap(ZoneMap.GetSinglePlane(zIndex), forAdmin, true, CentralRoom(zIndex));
        }

        /// <summary>
        /// The diameter of the zone
        /// </summary>
        /// <returns>the diameter of the zone in room count x,y,z</returns>
        public Tuple<int, int, int> Diameter()
        {
            return new Tuple<int, int, int>(ZoneMap.CoordinatePlane.GetUpperBound(0) - ZoneMap.CoordinatePlane.GetLowerBound(0)
                                            , ZoneMap.CoordinatePlane.GetUpperBound(1) - ZoneMap.CoordinatePlane.GetLowerBound(1)
                                            , ZoneMap.CoordinatePlane.GetUpperBound(2) - ZoneMap.CoordinatePlane.GetLowerBound(2));
        }

        /// <summary>
        /// Calculate the theoretical dimensions of the zone in inches
        /// </summary>
        /// <returns>height, width, depth in inches</returns>
        public Tuple<int, int, int> FullDimensions()
        {
            int height = -1, width = -1, depth = -1;

            return new Tuple<int, int, int>(height, width, depth);
        }

        public IEnumerable<string> RenderToLook(IEntity actor)
        {
            yield return String.Empty;
        }
    }
}
