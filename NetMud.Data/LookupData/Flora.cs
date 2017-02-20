using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Behaviors.Existential;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace NetMud.Data.LookupData
{
    /// <summary>
    /// Plants, all elements can be nullable (one has to exist)
    /// </summary>
    [Serializable]
    public class Flora : NaturalResourceDataPartial, IFlora
    {
        /// <summary>
        /// How much sunlight does this need to spawn
        /// </summary>
        public int SunlightPreference { get; set; }

        /// <summary>
        /// Does this plant go dormant in colder weather
        /// </summary>
        public bool Coniferous { get; set; }

        [JsonProperty("Wood")]
        private long _wood { get; set; }

        /// <summary>
        /// Bulk material of plant. Stem, trunk, etc.
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public IMaterial Wood
        { 
            get
            {
                return BackingDataCache.Get<IMaterial>(_wood);
            }
            set
            {
                _wood = value.ID;
            }
        }

        [JsonProperty("Flower")]
        private long _flower { get; set; }

        /// <summary>
        /// Flowering element of plant
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public IInanimateData Flower
        { 
            get
            {
                return BackingDataCache.Get<IInanimateData>(_flower);
            }
            set
            {
                _flower = value.ID;
            }
        }

        [JsonProperty("Leaf")]
        private long _leaf { get; set; }

        /// <summary>
        /// Leaves of the plant.
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public IInanimateData Leaf 
        { 
            get
            {
                return BackingDataCache.Get<IInanimateData>(_leaf);
            }
            set
            {
                _leaf = value.ID;
            }
        }

        [JsonProperty("Fruit")]
        private long _fruit { get; set; }

        /// <summary>
        /// Fruit of the plant, can be inedible like a pinecone
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public IInanimateData Fruit
        { 
            get
            {
                return BackingDataCache.Get<IInanimateData>(_fruit);
            }
            set
            {
                _fruit = value.ID;
            }
        }

        [JsonProperty("Seed")]
        private long _seed { get; set; }

        /// <summary>
        /// Seed of the plant.
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public IInanimateData Seed 
        { 
            get
            {
                return BackingDataCache.Get<IInanimateData>(_seed);
            }
            set
            {
                _seed = value.ID;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Flora()
        {

        }

        /// <summary>
        /// Gets the errors for data fitness
        /// </summary>
        /// <returns>a bunch of text saying how awful your data is</returns>
        public override IList<string> FitnessReport()
        {
            var dataProblems = base.FitnessReport();

            if (Wood == null)
                dataProblems.Add("Wood must have a value.");

            if (Flower == null && Seed == null && Leaf == null && Fruit == null)
                dataProblems.Add("At least one part of this plant must have a value.");

            return dataProblems;
        }

        public override bool CanSpawnIn(IGlobalPosition location)
        {
            var returnValue = true;

            return base.CanSpawnIn(location) && returnValue;
        }

        public override bool ShouldSpawnIn(IGlobalPosition location)
        {
            var returnValue = true;

            return base.ShouldSpawnIn(location) && returnValue;
        }
    }
}
