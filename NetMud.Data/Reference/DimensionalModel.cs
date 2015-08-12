using NetMud.DataAccess;
using NetMud.DataStructure.Base.Supporting;
using NetMud.Utility;
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
            int length = default(int);
            DataUtility.GetFromDataRow<int>(dr, "DimensionalModelLength", ref length);
            Length = length;

            int height = default(int);
            DataUtility.GetFromDataRow<int>(dr, "DimensionalModelHeight", ref height);
            Height = height;

            int width = default(int);
            DataUtility.GetFromDataRow<int>(dr, "DimensionalModelWidth", ref width);
            Width = width;

            long outDimModId = default(long);
            DataUtility.GetFromDataRow<long>(dr, "DimensionalModelID", ref outDimModId);
            ModelBackingData = ReferenceWrapper.GetOne<IDimensionalModelData>(outDimModId);
        }

        public DimensionalModel(int length, int height, int width, long backingDataId)
        {
            Length = length;
            Height = height;
            Width = width;
            ModelBackingData = ReferenceWrapper.GetOne<IDimensionalModelData>(backingDataId);
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
    }
}
