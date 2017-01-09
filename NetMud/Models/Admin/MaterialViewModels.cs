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
        
        [Display(Name = "Name")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [Display(Name = "Is Conductive")]
        public bool Conductive { get; set; }
        
        [Display(Name = "Is Magnetic")]
        public bool Magnetic { get; set; }

        [Display(Name = "Is Flammable")]
        public bool Flammable { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Viscosity")]
        [DataType(DataType.Text)]
        public short Viscosity { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Density")]
        [DataType(DataType.Text)]
        public short Density { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Mallebility")]
        [DataType(DataType.Text)]
        public short Mallebility { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Ductility")]
        [DataType(DataType.Text)]
        public short Ductility { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Porosity")]
        [DataType(DataType.Text)]
        public short Porosity { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Fusion Point")]
        [DataType(DataType.Text)]
        public short SolidPoint { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Vaporization Point")]
        [DataType(DataType.Text)]
        public short GasPoint { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Temperature Retention")]
        [DataType(DataType.Text)]
        public short TemperatureRetention { get; set; }

        [Display(Name = "Damage Resistance")]
        public short[] Resistances { get; set; }

        [Display(Name = "Damage Resistance Value")]
        public short[] ResistanceValues { get; set; }

        [Display(Name = "Material Composition")]
        public long[] Compositions { get; set; }

        [Display(Name = "Material Composition Percentage")]
        public short[] CompositionPercentages { get; set; }

        [StringLength(2000, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Help Text Body")]
        public string HelpBody { get; set; }

        public IEnumerable<IMaterial> ValidMaterials { get; set; }
        public Material DataObject { get; set; }
    }
}