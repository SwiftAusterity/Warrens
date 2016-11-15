using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Supporting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace NetMud.Data.LookupData
{
    /// <summary>
    /// Plants, all elements can be nullable (one has to exist)
    /// </summary>
    [Serializable]
    public class Flora : NaturalResourceDataPartial, IFlora
    {
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
        public IMaterial Flower
        { 
            get
            {
                return BackingDataCache.Get<IMaterial>(_flower);
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
        public IMaterial Leaf 
        { 
            get
            {
                return BackingDataCache.Get<IMaterial>(_leaf);
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
        public IMaterial Fruit
        { 
            get
            {
                return BackingDataCache.Get<IMaterial>(_fruit);
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
        public IMaterial Seed 
        { 
            get
            {
                return BackingDataCache.Get<IMaterial>(_seed);
            }
            set
            {
                _seed = value.ID;
            }
        }
    }
}
