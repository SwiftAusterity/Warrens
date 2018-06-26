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
        
        [Display(Name = "Name", Description = "The descriptive name of this material. Used to find and refer to it.")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [Display(Name = "Conductive", Description = "Does this conduct electricity?")]
        public bool Conductive { get; set; }
        
        [Display(Name = "Magnetic", Description = "Is this magnetic?")]
        public bool Magnetic { get; set; }

        [Display(Name = "Flammable", Description = "Is this flammable?")]
        public bool Flammable { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Viscosity", Description = "How easily this material flows across other materials. High viscosity = flows less easily.")]
        [DataType(DataType.Text)]
        public short Viscosity { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Density", Description = "The density of this material. High density = higher weight of objects. Also factors into damage resistance.")]
        [DataType(DataType.Text)]
        public short Density { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Mallebility", Description = "How easily this material is deformed by impact damage without breaking or fracturing.")]
        [DataType(DataType.Text)]
        public short Mallebility { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Ductility", Description = "How easily this material is deformed by tensile stress (stretching, bending) without breaking.")]
        [DataType(DataType.Text)]
        public short Ductility { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Porosity", Description = "How porous (full of holes) this material is when formed. Highly porous materials may become pass thru for small objects and creatures.")]
        [DataType(DataType.Text)]
        public short Porosity { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Fusion Point", Description = "The temperature at which this material becomes a solid.")]
        [DataType(DataType.Text)]
        public short SolidPoint { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Vaporization Point", Description = "The temperature at which this material becomes a gas.")]
        [DataType(DataType.Text)]
        public short GasPoint { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Temperature Retention", Description = "How well does this material retain its temperature.")]
        [DataType(DataType.Text)]
        public short TemperatureRetention { get; set; }

        [Display(Name = "Damage Resistance", Description = "The type of damage this has special resistances to.")]
        public short[] Resistances { get; set; }

        [Display(Name = "Damage Resistance Value", Description = "The factor this material is resistant to the damage.")]
        public short[] ResistanceValues { get; set; }

        [Display(Name = "Material Composition", Description = "Is this material an alloy of other materials?")]
        public long[] Compositions { get; set; }

        [Display(Name = "Material Composition Percentage", Description = "The percentage of the whole this material is the composition of.")]
        public short[] CompositionPercentages { get; set; }

        [StringLength(2000, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Help Text Body", Description = "The descriptive text shown on the materials list page and when help is used in game.")]
        public string HelpBody { get; set; }

        public IEnumerable<IMaterial> ValidMaterials { get; set; }
        public Material DataObject { get; set; }
    }
}