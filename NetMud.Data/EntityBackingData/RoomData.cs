using NetMud.Data.Game;
using NetMud.Data.LookupData;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.Supporting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.EntityBackingData
{
    /// <summary>
    /// Backing data for Rooms
    /// </summary>
    [Serializable]
    public class RoomData : EntityBackingDataPartial, IRoomData
    {
        /// <summary>
        /// The system type of data this attaches to
        /// </summary>
        public override Type EntityClass
        {
            get { return typeof(Room); }
        }

        /// <summary>
        /// Framework for the physics model of an entity
        /// </summary>
        public IDimensionalModel Model { get; set; }

        [JsonProperty("Medium")]
        private long _medium { get; set; }

        /// <summary>
        /// What is in the middle of the room
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IMaterial Medium
        {
            get
            {
                return BackingDataCache.Get<IMaterial>(_medium);
            }
            set
            {
                if(value != null)
                    _medium = value.ID;
            }
        }

        [JsonProperty("ZoneAffiliation")]
        private long _zoneAffiliation { get; set; }

        /// <summary>
        /// What zone this belongs to
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IZone ZoneAffiliation
        {
            get
            {
                return BackingDataCache.Get<IZone>(_zoneAffiliation);
            }
            set
            {
                if(value != null)
                    _zoneAffiliation = value.ID;
            }
        }

        [JsonProperty("Borders")]
        private IDictionary<string, long> _borders { get; set; }

        /// <summary>
        /// What the walls are made of
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IDictionary<string, IMaterial> Borders
        {
            get
            {
                if (_borders != null)
                    return _borders.ToDictionary(k => k.Key, k => BackingDataCache.Get<IMaterial>(k.Value));

                return null;
            }
            set
            {
                if (value == null)
                    return;

                _borders = value.ToDictionary(k => k.Key, k => k.Value.ID);
            }
        }

        /// <summary>
        /// Current coordinates of the room on its world map
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public Tuple<int, int, int> Coordinates { get; set; }

        /// <summary>
        /// Spawn new room with its model
        /// </summary>
        [JsonConstructor]
        public RoomData(DimensionalModel model)
        {
            Model = model;
        }

        /// <summary>
        /// Blank constructor
        /// </summary>
        public RoomData()
        {

        }

        /// <summary>
        /// Gets the remaining distance to the destination room
        /// </summary>
        /// <param name="destination">The room you're heading for</param>
        /// <returns>distance (in rooms) between here and there</returns>
        public int GetDistanceToRoom(IRoomData destination)
        {
            return -1;
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
        /// What pathways are affiliated with this room data (what it spawns with)
        /// </summary>
        /// <param name="withReturn">includes paths into this room as well</param>
        /// <returns>the valid pathways</returns>
        public IEnumerable<IPathwayData> GetPathways(bool withReturn = false)
        {
            return BackingDataCache.GetAll<IPathwayData>().Where(path => path.FromLocationID.Equals(ID.ToString()) 
                                                                        || (withReturn && path.ToLocationID.Equals(ID.ToString()))
                                                                        );
        }
    }
}
