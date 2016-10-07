using NetMud.Authentication;
using NetMud.Data.LookupData;
using NetMud.DataStructure.Base.Supporting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace NetMud.Models.Admin
{
    public class ManageDimensionalModelDataViewModel : PagedDataModel<DimensionalModelData>, BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageDimensionalModelDataViewModel(IEnumerable<DimensionalModelData> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<DimensionalModelData, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }
    }

    public class AddEditDimensionalModelDataViewModel : BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditDimensionalModelDataViewModel()
        {
        }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        
        [Display(Name = "Name")]
        public string NewName { get; set; }

        
        [Display(Name = "ModelType")]
        public DimensionalModelType NewModelType { get; set; }

        [DataType(DataType.Upload)]
        [Display(Name = "Model Planes Upload")]
        public HttpPostedFileBase ModelFile { get; set; }

        public DimensionalModelData DataObject { get; set; }
    }
}