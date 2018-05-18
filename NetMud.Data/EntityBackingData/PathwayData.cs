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
        public override Type EntityClass
        {
            get { return typeof(Game.Pathway); }
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
        /// 0->360 degrees with 0 being absolute north (meaning 90 is west, 180 south, etc) -1 means no cardinality
        /// </summary>
        public int DegreesFromNorth { get; set; }

        /// <summary>
        /// -100 to 100 (negative being a decline) % grade of up and down
        /// </summary>
        public int InclineGrade { get; set; }

        [JsonProperty("ToLocation")]
        private long _toLocation { get; set; }

        /// <summary>
        /// What is in the middle of the room
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [NonNullableDataIntegrity("To Location is invalid.")]
        public IRoomData ToLocation
        {
            get
            {
                return BackingDataCache.Get<IRoomData>(_toLocation);
            }
            set
            {
                if (value != null)
                    _toLocation = value.ID;
            }
        }

        [JsonProperty("FromLocation")]
        private long _fromLocation { get; set; }

        /// <summary>
        /// What is in the middle of the room
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [NonNullableDataIntegrity("From Location is invalid.")]
        public IRoomData FromLocation
        {
            get
            {
                return BackingDataCache.Get<IRoomData>(_fromLocation);
            }
            set
            {
                if (value != null)
                    _fromLocation = value.ID;
            }
        }

        /// <summary>
        /// Framework for the physics model of an entity
        /// </summary>
        [NonNullableDataIntegrity("Physical Model is invalid.")]
        public IDimensionalModel Model { get; set; }

        /// <summary>
        /// The visual output of using this path
        /// </summary>
        public ILexica VisualOutput { get; set; }

        /// <summary>
        /// The auditory output of using this path
        /// </summary>
        public ILexica AuditoryOutput { get; set; }

        /// <summary>
        /// The auditory output of using this path
        /// </summary>
        public ILexica OlefactoryOutput { get; set; }

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
    }
}
