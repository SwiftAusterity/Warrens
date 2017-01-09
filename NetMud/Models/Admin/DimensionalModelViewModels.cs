using NetMud.Authentication;
using NetMud.DataStructure.Base.Supporting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace NetMud.Models.Admin
{
    public class ManageDimensionalModelDataViewModel : PagedDataModel<IDimensionalModelData>, BaseViewModel
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

    public class AddEditDimensionalModelDataViewModel : BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditDimensionalModelDataViewModel()
        {
        }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Name")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [Display(Name = "Model Type")]
        [UIHint("EnumDropDownList")]
        public DimensionalModelType ModelType { get; set; }

        [DataType(DataType.Upload)]
        [Display(Name = "Model Planes Upload")]
        public HttpPostedFileBase ModelFile { get; set; }

        [Display(Name = "X-Plane")]
        public short[] CoordinateDamageTypes { get; set; }

        [Display(Name = "Name")]
        public string[] ModelPlaneNames { get; set; }

        public IDimensionalModelData DataObject { get; set; }
    }

    public class DimensionalEntityEditViewModel : BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        [Range(1, 1200, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Length (inches)")]
        [DataType(DataType.Text)]
        public int DimensionalModelLength { get; set; }

        [Range(1, 1200, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Height (inches)")]
        [DataType(DataType.Text)]
        public int DimensionalModelHeight { get; set; }

        [Range(1, 1200, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Width (inches)")]
        [DataType(DataType.Text)]
        public int DimensionalModelWidth { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Hollowness")]
        [DataType(DataType.Text)]
        public int DimensionalModelVacuity { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Surface Cavitation")]
        [DataType(DataType.Text)]
        public int DimensionalModelCavitation { get; set; }
    }

    public class TwoDimensionalEntityEditViewModel : DimensionalEntityEditViewModel
    {
        [Display(Name = "Dimensional Model")]
        public long DimensionalModelId { get; set; }

        [Display(Name = "Part Name")]
        public string[] ModelPartNames { get; set; }

        [Display(Name = "Material")]
        public long[] ModelPartMaterials { get; set; }

        public IEnumerable<IDimensionalModelData> ValidModels { get; set; }
        public IEnumerable<IMaterial> ValidMaterials { get; set; }
        public IDimensionalModel ModelDataObject { get; set; }
    }
}