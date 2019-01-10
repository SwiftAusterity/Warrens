using NetMud.Data.Architectural.DataIntegrity;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.NaturalResource;
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
        private TemplateCacheKey _wood { get; set; }

        /// <summary>
        /// Bulk material of plant. Stem, trunk, etc.
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        [NonNullableDataIntegrity("Wood must have a value.")]
        public IMaterial Wood
        { 
            get
            {
                return TemplateCache.Get<IMaterial>(_wood);
            }
            set
            {
                _wood = new TemplateCacheKey(value);
            }
        }

        [JsonProperty("Flower")]
        private TemplateCacheKey _flower { get; set; }

        /// <summary>
        /// Flowering element of plant
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public IInanimateTemplate Flower
        { 
            get
            {
                return TemplateCache.Get<IInanimateTemplate>(_flower);
            }
            set
            {
                _flower = new TemplateCacheKey(value);
            }
        }

        [JsonProperty("Leaf")]
        private TemplateCacheKey _leaf { get; set; }

        /// <summary>
        /// Leaves of the plant.
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public IInanimateTemplate Leaf 
        { 
            get
            {
                return TemplateCache.Get<IInanimateTemplate>(_leaf);
            }
            set
            {
                _leaf = new TemplateCacheKey(value);
            }
        }

        [JsonProperty("Fruit")]
        private TemplateCacheKey _fruit { get; set; }

        /// <summary>
        /// Fruit of the plant, can be inedible like a pinecone
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public IInanimateTemplate Fruit
        { 
            get
            {
                return TemplateCache.Get<IInanimateTemplate>(_fruit);
            }
            set
            {
                _fruit = new TemplateCacheKey(value);
            }
        }

        [JsonProperty("Seed")]
        private TemplateCacheKey _seed { get; set; }

        /// <summary>
        /// Seed of the plant.
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public IInanimateTemplate Seed 
        { 
            get
            {
                return TemplateCache.Get<IInanimateTemplate>(_seed);
            }
            set
            {
                _seed = new TemplateCacheKey(value);
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

            if (Flower == null && Seed == null && Leaf == null && Fruit == null)
                dataProblems.Add("At least one part of this plant must have a value.");

            return dataProblems;
        }

        /// <summary>
        /// Get the significant details of what needs approval
        /// </summary>
        /// <returns>A list of strings</returns>
        public override IDictionary<string, string> SignificantDetails()
        {
            var returnList = base.SignificantDetails();

            returnList.Add("Sunlight", SunlightPreference.ToString());
            returnList.Add("Coniferous", Coniferous.ToString());
            returnList.Add("Wood", Wood.Name);
            returnList.Add("Flower", Flower.Name);
            returnList.Add("Leaf", Leaf.Name);
            returnList.Add("Fruit", Fruit.Name);
            returnList.Add("Seed", Seed.Name);

            return returnList;
        }
    }
}
