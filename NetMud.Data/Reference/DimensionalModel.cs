using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Supporting;
using NetMud.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace NetMud.Data.Reference
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

        [JsonProperty("BackingDataId")]
        private long _backingDataId { get; set; }

        /// <summary>
        /// The model we're following
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IDimensionalModelData ModelBackingData
        {
            get
            {
                if (_backingDataId > 0)
                    return BackingDataCache.Get<IDimensionalModelData>(_backingDataId);
                else
                {
                    // 0d models don't have real values
                    var returnValue = new DimensionalModelData();
                    returnValue.ModelType = DimensionalModelType.None;

                    return returnValue;
                }

                return null;
            }
            set
            {
                if (value == null)
                    return;

                _backingDataId = value.ID;
            }
        }

        [JsonProperty("Composition")]
        private IDictionary<string, long> _composition { get; set; }

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

                _composition = value.ToDictionary(k => k.Key, k => k.Value.ID);
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
        /// <param name="backingDataId">dimensional model backing data id</param>
        /// <param name="materialComps">The material compositions</param>
        public DimensionalModel(int length, int height, int width, long backingDataId, IDictionary<string, IMaterial> materialComps)
        {
            Length = length;
            Height = height;
            Width = width;
            Composition = materialComps;

            ModelBackingData = ReferenceWrapper.GetOne<DimensionalModelData>(backingDataId);
        }

        /// <summary>
        /// Constructor for dimensional model based on full specific data
        /// </summary>
        /// <param name="length">Length parameter of the model</param>
        /// <param name="height">Height parameter of the model</param>
        /// <param name="width">Width parameter of the model</param>
        /// <param name="backingDataId">dimensional model backing data id</param>
        /// <param name="compJson">The material compositions in json form</param>
        public DimensionalModel(int length, int height, int width, long backingDataId, string compJson)
        {
            Length = length;
            Height = height;
            Width = width;
            Composition = DeserializeMaterialCompositions(compJson);

            ModelBackingData = ReferenceWrapper.GetOne<DimensionalModelData>(backingDataId);
        }

        /// <summary>
        /// Constructor for dimensional model based on full specific data
        /// </summary>
        /// <param name="length">Length parameter of the model</param>
        /// <param name="height">Height parameter of the model</param>
        /// <param name="width">Width parameter of the model</param>
        /// <param name="backingDataId">dimensional model backing data id</param>
        /// <param name="compJson">The material compositions in json form</param>
        /// <param name="modelJson">the model structure json</param>
        public DimensionalModel(int length, int height, int width, string modelJson, long backingDataId, string compJson)
        {
            Length = length;
            Height = height;
            Width = width;
            Composition = DeserializeMaterialCompositions(compJson);

            ModelBackingData = new DimensionalModelData(backingDataId, modelJson);
        }

        /// <summary>
        /// constructor for 0 dimension models
        /// </summary>
        /// <param name="length">Length parameter of the model</param>
        /// <param name="height">Height parameter of the model</param>
        /// <param name="width">Width parameter of the model</param>
        public DimensionalModel(int length, int height, int width)
        {
            Length = length;
            Height = height;
            Width = width;
            Composition = new Dictionary<string, IMaterial>();

            ModelBackingData = new DimensionalModelData();
            ModelBackingData.ModelType = DimensionalModelType.None;
        }
    }
}
