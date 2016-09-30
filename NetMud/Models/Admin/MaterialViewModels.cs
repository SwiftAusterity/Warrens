using NetMud.Authentication;
using NetMud.Data.LookupData;
using NetMud.DataStructure.Base.Supporting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;


namespace NetMud.Models.Admin
{
    public class ManageMaterialDataViewModel : PagedDataModel<Material>, BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageMaterialDataViewModel(IEnumerable<Material> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<Material, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower()) || item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }
    }

    public class AddEditMaterialViewModel : BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditMaterialViewModel()
        {
            ValidMaterials = Enumerable.Empty<IMaterial>();
        }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [DataType(DataType.Text)]
        [Display(Name = "Name")]
        public string NewName { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Is Conductive")]
        public bool NewConductive { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Is Magnetic")]
        public bool NewMagnetic { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Is Flammable")]
        public bool NewFlammable { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [DataType(DataType.Text)]
        [Display(Name = "Viscosity")]
        public short NewViscosity { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [DataType(DataType.Text)]
        [Display(Name = "Density")]
        public short NewDensity { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [DataType(DataType.Text)]
        [Display(Name = "Mallebility")]
        public short NewMallebility { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [DataType(DataType.Text)]
        [Display(Name = "Ductility")]
        public short NewDuctility { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [DataType(DataType.Text)]
        [Display(Name = "Porosity")]
        public short NewPorosity { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [DataType(DataType.Text)]
        [Display(Name = "Fusion Point")]
        public short NewSolidPoint { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [DataType(DataType.Text)]
        [Display(Name = "Vaporization Point")]
        public short NewGasPoint { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [DataType(DataType.Text)]
        [Display(Name = "Temperature Retention")]
        public short NewTemperatureRetention { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Damage Resistance")]
        public short[] Resistances { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Damage Resistance Value")]
        public short[] ResistanceValues { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Material Composition")]
        public long[] Compositions { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Material Composition Percentage")]
        public short[] CompositionPercentages { get; set; }

        [StringLength(2000, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Help Text Body")]
        public string NewHelpBody { get; set; }

        public IEnumerable<IMaterial> ValidMaterials { get; set; }
        public Material DataObject { get; set; }
    }
}