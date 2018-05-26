using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Supporting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.LookupData
{
    /* Does not need to be ScriptIgnored */

    /// <summary>
    /// The live version of a dimensional model
    /// </summary>
    [Serializable]
    public class DimensionalModel : IDimensionalModel
    {
        /// <summary>
        /// Y axis of the 11 plane model
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// Measurement of all 11 planes vertically
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// X axis of the 11 plane model
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// How hollow something is, we have to maintain current vacuity versus the spawned vacuity in the ModelData
        /// </summary>
        public int Vacuity { get; set; }

        /// <summary>
        /// How pock-marked the surface areas are of the object
        /// </summary>
        public int SurfaceCavitation { get; set; }

        [JsonProperty("ModelBackingData")]
        private BackingDataCacheKey _modelBackingData { get; set; }

        /// <summary>
        /// The model we're following
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IDimensionalModelData ModelBackingData
        {
            get
            {
                if (_modelBackingData != null)
                    return BackingDataCache.Get<IDimensionalModelData>(_modelBackingData);
                else
                {
                    // 0d models don't have real values
                    var returnValue = new DimensionalModelData
                    {
                        ModelType = DimensionalModelType.None
                    };

                    return returnValue;
                }
            }
            set
            {
                if (value == null)
                    return;

                _modelBackingData = new BackingDataCacheKey(value);
            }
        }

        [JsonProperty("Composition")]
        private IDictionary<string, BackingDataCacheKey> _composition { get; set; }

        /// <summary>
        /// Collection of model section name to material composition mappings
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IDictionary<string, IMaterial> Composition
        {
            get
            {
                if (_composition != null)
                    return _composition.ToDictionary(k => k.Key, k => BackingDataCache.Get<IMaterial>(k.Value));

                return null;
            }
            set
            {
                if (value == null)
                    return;

                _composition = value.ToDictionary(k =>k.Key, k => new BackingDataCacheKey(k.Value));
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public DimensionalModel()
        {

        }

        /// <summary>
        /// Constructor for dimensional model based on full specific data
        /// </summary>
        /// <param name="length">Length parameter of the model</param>
        /// <param name="height">Height parameter of the model</param>
        /// <param name="width">Width parameter of the model</param>
        /// <param name="backingDataKey">dimensional model backing data id</param>
        /// <param name="materialComps">The material compositions</param>
        public DimensionalModel(int length, int height, int width, int vacuity, int surfaceCavitation, BackingDataCacheKey backingDataKey, IDictionary<string, IMaterial> materialComps)
        {
            Length = length;
            Height = height;
            Width = width;
            Vacuity = vacuity;
            SurfaceCavitation = surfaceCavitation;
            Composition = materialComps;

            _modelBackingData = backingDataKey;
        }

        /// <summary>
        /// constructor for 0 dimension models
        /// </summary>
        /// <param name="length">Length parameter of the model</param>
        /// <param name="height">Height parameter of the model</param>
        /// <param name="width">Width parameter of the model</param>
        public DimensionalModel(int length, int height, int width, int vacuity, int surfaceCavitation)
        {
            Length = length;
            Height = height;
            Width = width;
            Vacuity = vacuity;
            SurfaceCavitation = surfaceCavitation;
            Composition = new Dictionary<string, IMaterial>();

            ModelBackingData = new DimensionalModelData
            {
                ModelType = DimensionalModelType.None
            };
        }
    }
}
