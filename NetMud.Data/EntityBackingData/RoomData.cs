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

        [JsonProperty("Affiliation")]
        private long _affiliation { get; set; }

        /// <summary>
        /// What zone this belongs to
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public ILocaleData Affiliation
        {
            get
            {
                return BackingDataCache.Get<ILocaleData>(_affiliation);
            }
            set
            {
                if(value != null)
                    _affiliation = value.ID;
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
        /// Gets the errors for data fitness
        /// </summary>
        /// <returns>a bunch of text saying how awful your data is</returns>
        public override IList<string> FitnessReport()
        {
            var dataProblems = base.FitnessReport();

            if (Model == null)
                dataProblems.Add("Physical Model is invalid.");

            if (Medium == null)
                dataProblems.Add("Medium material is invalid.");

            if (Affiliation == null)
                dataProblems.Add("Affiliation is invalid.");

            if (Coordinates?.Item1 < 0 || Coordinates?.Item2 < 0 || Coordinates?.Item3 < 0)
                dataProblems.Add("Coordinates are invalid.");

            return dataProblems;
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
