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
        [Display(Name = "Name", Description = "The descriptive name used to refer to this.")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [StringLength(2000, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 20)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Help Text", Description = "The descriptive text shown on the list page and in the help system for this.")]
        public string HelpText { get; set; }

        [Display(Name = "Model Type", Description = "The type of model this is. Flat models are used for everything.")]
        [UIHint("EnumDropDownList")]
        public DimensionalModelType ModelType { get; set; }

        [DataType(DataType.Upload)]
        [Display(Name = "Model Planes Upload", Description = "Upload a txt file comprised of 11 rows of 11 characters separated by commas.")]
        public HttpPostedFileBase ModelFile { get; set; }

        [Display(Name = "X-Plane", Description = "One row of model characters.")]
        [UIHint("EnumDropDownList")]
        public short[] CoordinateDamageTypes { get; set; }

        [Display(Name = "Name", Description = "Descriptive name for this Y-axis row. Things like Blade, Hilt, Handle, etc.")]
        public string[] ModelPlaneNames { get; set; }

        public IDimensionalModelData DataObject { get; set; }
    }

    public class DimensionalEntityEditViewModel : BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        [Range(1, 1200, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Length (inches)", Description = "The dimensional length of this model.")]
        [DataType(DataType.Text)]
        public int DimensionalModelLength { get; set; }

        [Range(1, 1200, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Height (in)", Description = "The dimensional height of this model.")]
        [DataType(DataType.Text)]
        public int DimensionalModelHeight { get; set; }

        [Range(1, 1200, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Width (in)", Description = "The dimensional width of this model.")]
        [DataType(DataType.Text)]
        public int DimensionalModelWidth { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Hollowness", Description = "The hollowness of the model. Hollowness can increase sturctural integrity up to a point.")]
        [DataType(DataType.Text)]
        public int DimensionalModelVacuity { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Surface Cavitation", Description = "The cavitation of the model surface. Cavitation can decrease sturctural integrity and account for things passing through.")]
        [DataType(DataType.Text)]
        public int DimensionalModelCavitation { get; set; }
    }

    public class TwoDimensionalEntityEditViewModel : DimensionalEntityEditViewModel
    {
        [Display(Name = "Dimensional Model", Description = "The Model used for this")]
        public long DimensionalModelId { get; set; }

        [Display(Name = "Part Name", Description = "The name of the part.")]
        public string[] ModelPartNames { get; set; }

        [Display(Name = "Material", Description = "The material this part is made of.")]
        public long[] ModelPartMaterials { get; set; }

        public IEnumerable<IDimensionalModelData> ValidModels { get; set; }
        public IEnumerable<IMaterial> ValidMaterials { get; set; }
        public IDimensionalModel ModelDataObject { get; set; }
    }
}