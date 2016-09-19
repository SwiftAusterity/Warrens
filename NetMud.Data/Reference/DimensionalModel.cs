using NetMud.DataAccess; using NetMud.DataAccess.Cache;
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
        public IDimensionalModelData ModelBackingData 
        {
            get
            {
              if (_backingDataId >= 0)
                 return BackingDataCache.Get<IDimensionalModelData>(_backingDataId);

                return null;
            }
            set
            {
                if (value == null)
                    return;

                _backingDataId = value.ID;
            }
        }

        [JsonProperty("MaterialComposition")]
        private IDictionary<string, long> _materialComposition { get; set; }

        /// <summary>
        /// Collection of model section name to material composition mappings
        /// </summary>
        [ScriptIgnore]
        public IDictionary<string, IMaterial> Composition 
        {
            get
            {
                if (_materialComposition != null)
                    return _materialComposition.ToDictionary(k => k.Key, k => BackingDataCache.Get<IMaterial>(k.Value));

                return null;
            }
            set
            {
                if (value == null)
                    return;

                _materialComposition = value.ToDictionary(k => k.Key, k => k.Value.ID);
            }
        }

        /// <summary>
        /// Constructor for dimensional model based on a datarow
        /// </summary>
        /// <param name="dr">the row from the db</param>
        public DimensionalModel(global::System.Data.DataRow dr)
        {
            Length = DataUtility.GetFromDataRow<int>(dr, "DimensionalModelLength");
            Height = DataUtility.GetFromDataRow<int>(dr, "DimensionalModelHeight");
            Width = DataUtility.GetFromDataRow<int>(dr, "DimensionalModelWidth");

            long outDimModId = DataUtility.GetFromDataRow<long>(dr, "DimensionalModelID");

            if (outDimModId > 0)
            {
                ModelBackingData = ReferenceWrapper.GetOne<DimensionalModelData>(outDimModId);

                string materialComps = DataUtility.GetFromDataRow<string>(dr, "DimensionalModelMaterialCompositions");
                Composition = DeserializeMaterialCompositions(materialComps);
            }
            else //0 dimensional models don't have an actual model
            {
                ModelBackingData = new DimensionalModelData();
                ModelBackingData.ModelType = DimensionalModelType.None;
                Composition = new Dictionary<string, IMaterial>();
            }
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

        /// <summary>
        /// Turn the material composition set into a json string
        /// </summary>
        /// <returns>the json in a string</returns>
        public string SerializeMaterialCompositions()
        {
            var materialComps = new Dictionary<string, long>();

            foreach (var kvp in Composition)
                materialComps.Add(kvp.Key, kvp.Value.ID);

            return JsonConvert.SerializeObject(materialComps);
        }

        /// <summary>
        /// Turn json string of material composition into its proper object form
        /// </summary>
        /// <param name="compJson">the json in a string</param>
        /// <returns>the object form</returns>
        private IDictionary<string, IMaterial> DeserializeMaterialCompositions(string compJson)
        {
            var composition = new Dictionary<string, IMaterial>();

            dynamic comps = JsonConvert.DeserializeObject(compJson);

            foreach (dynamic comp in comps)
            {
                string sectionName = comp.Name;
                long materialId = comp.Value;

                var material = ReferenceWrapper.GetOne<Material>(materialId);

                if (material != null && !string.IsNullOrWhiteSpace(sectionName))
                    composition.Add(sectionName, material);
            }

            return composition;
        }
    }
}
