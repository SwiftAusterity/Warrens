using NetMud.Authentication;
using NetMud.DataStructure.Architectural.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.Admin
{
    public class ManageDimensionalModelDataViewModel : PagedDataModel<IDimensionalModelData>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageDimensionalModelDataViewModel(IEnumerable<IDimensionalModelData> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IDimensionalModelData, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }
    }

    public class AddEditDimensionalModelDataViewModel : IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditDimensionalModelDataViewModel()
        {
        }

        [UIHint("DimensionalModelData")]
        public IDimensionalModelData DataObject { get; set; }
    }

    public class DimensionalEntityEditViewModel : IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }
    }

    public class TwoDimensionalEntityEditViewModel : DimensionalEntityEditViewModel
    {
        public IEnumerable<IDimensionalModelData> ValidModels { get; set; }
        public IEnumerable<IMaterial> ValidMaterials { get; set; }
    }
}