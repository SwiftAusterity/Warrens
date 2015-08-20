using NetMud.DataAccess;
using NetMud.DataStructure.Base.Supporting;
using NetMud.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.Data.Reference
{
    public class DimensionalModel : IDimensionalModel
    {
        public DimensionalModel(global::System.Data.DataRow dr)
        {
            Length = DataUtility.GetFromDataRow<int>(dr, "DimensionalModelLength");
            Height = DataUtility.GetFromDataRow<int>(dr, "DimensionalModelHeight");
            Width = DataUtility.GetFromDataRow<int>(dr, "DimensionalModelWidth");

            long outDimModId = DataUtility.GetFromDataRow<long>(dr, "DimensionalModelID");
            ModelBackingData = ReferenceWrapper.GetOne<DimensionalModelData>(outDimModId);

            string materialComps = DataUtility.GetFromDataRow<string>(dr, "DimensionalModelMaterialCompositions");
            Composition = DeserializeMaterialCompositions(materialComps);
        }

        public DimensionalModel(int length, int height, int width, long backingDataId, IDictionary<string, IMaterial> materialComps)
        {
            Length = length;
            Height = height;
            Width = width;
            Composition = materialComps;

            ModelBackingData = ReferenceWrapper.GetOne<DimensionalModelData>(backingDataId);
        }

        public DimensionalModel(int length, int height, int width, long backingDataId, string compJson)
        {
            Length = length;
            Height = height;
            Width = width;
            Composition = DeserializeMaterialCompositions(compJson);

            ModelBackingData = ReferenceWrapper.GetOne<DimensionalModelData>(backingDataId);
        }

        public DimensionalModel(int length, int height, int width, string modelJson, long backingDataId, string compJson)
        {
            Length = length;
            Height = height;
            Width = width;
            Composition = DeserializeMaterialCompositions(compJson);

            ModelBackingData = new DimensionalModelData(backingDataId, modelJson);
        }

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
        /// The model we're following
        /// </summary>
        public IDimensionalModelData ModelBackingData { get; set; }

        /// <summary>
        /// Collection of model section name to material composition mappings
        /// </summary>
        public IDictionary<string, IMaterial> Composition { get; set; }

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

        public string SerializeMaterialCompositions()
        {
            var materialComps = new Dictionary<string, long>();

            foreach (var kvp in Composition)
                materialComps.Add(kvp.Key, kvp.Value.ID);

            return JsonConvert.SerializeObject(materialComps);
        }
    }
}
