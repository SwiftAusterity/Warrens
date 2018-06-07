using NetMud.DataStructure.Base.Supporting;
using System.Collections.Generic;

namespace NetMud.Models.Admin.PartialInterfaces
{
    public interface ITwoDimensionalEntityViewModel : IDimensionalEntityViewModel
    {
        long DimensionalModelId { get; set; }
        string[] ModelPartNames { get; set; }
        long[] ModelPartMaterials { get; set; }
        IEnumerable<IDimensionalModelData> ValidModels { get; set; }
        IEnumerable<IMaterial> ValidMaterials { get; set; }
        IDimensionalModel ModelDataObject { get; set; }
    }
}
