﻿using NetMud.Data.LookupData;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Cartography;
using Newtonsoft.Json;
using System;
using System.Web.Script.Serialization;
using NetMud.Data.DataIntegrity;
using NetMud.DataAccess.Cache;
using System.Collections.Generic;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Behaviors.Rendering;

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
        [JsonIgnore]
        [ScriptIgnore]
        public override Type EntityClass
        {
            get { return typeof(Game.Pathway); }
        }

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
                    _keywords = new string[] { Name };

                return _keywords;
            }
            set { _keywords = value; }
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
        /// Set of output relevant to this exit
        /// </summary>
        public HashSet<IOccurrence> Occurrences { get; set; }

        /// <summary>
        /// 0->360 degrees with 0 being absolute north (meaning 90 is west, 180 south, etc) -1 means no cardinality
        /// </summary>
        public int DegreesFromNorth { get; set; }

        /// <summary>
        /// -100 to 100 (negative being a decline) % grade of up and down
        /// </summary>
        public int InclineGrade { get; set; }

        [JsonProperty("Destination")]
        private BackingDataCacheKey _destination { get; set; }

        /// <summary>
        /// What is in the middle of the room
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [NonNullableDataIntegrity("Destination is invalid.")]
        public ILocationData Destination
        {
            get
            {
                return (ILocationData)BackingDataCache.Get(_destination);
            }
            set
            {
                if (value != null)
                    _destination = new BackingDataCacheKey(value);
            }
        }

        [JsonProperty("Origin")]
        private BackingDataCacheKey _origin { get; set; }

        /// <summary>
        /// What is in the middle of the room
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [NonNullableDataIntegrity("Origin is invalid.")]
        public ILocationData Origin
        {
            get
            {
                return (ILocationData)BackingDataCache.Get(_origin);
            }
            set
            {
                if (value != null)
                    _origin = new BackingDataCacheKey(value);
            }
        }

        /// <summary>
        /// Framework for the physics model of an entity
        /// </summary>
        [NonNullableDataIntegrity("Physical Model is invalid.")]
        public IDimensionalModel Model { get; set; }

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

        /// <summary>
        /// Get the live version of this in the world
        /// </summary>
        /// <returns>The live data</returns>
        public IPathway GetLiveInstance()
        {
            return LiveCache.Get<IPathway>(Id);
        }
    }
}
