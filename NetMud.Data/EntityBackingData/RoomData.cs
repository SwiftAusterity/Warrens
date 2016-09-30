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
            get { return typeof(NetMud.Data.Game.Room); }
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

        /// <summary>
        /// What walls are made of
        /// </summary>
        [JsonProperty("Borders")]
        private IDictionary<string, long> _borders { get; set; }

        /// <summary>
        /// The list of internal compositions for separate/explosion/sharding
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
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        public override Tuple<int, int, int> GetModelDimensions()
        {
            return new Tuple<int, int, int>(Model.Height, Model.Length, Model.Width);
        }
    }
}
