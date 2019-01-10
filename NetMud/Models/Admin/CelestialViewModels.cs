using NetMud.Authentication;
using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Gaia;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.Admin
{
    public class ManageCelestialsViewModel : PagedDataModel<ICelestial>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageCelestialsViewModel(IEnumerable<ICelestial> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<ICelestial, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }

    }

    public class AddEditCelestialViewModel : AddContentModel<ICelestial>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        [Display(Name = "Apply Existing Template", Description = "Apply an existing object's data to this new data.")]
        [UIHint("CelestialList")]
        [CelestialDataBinder]
        public override ICelestial Template { get; set; }

        public AddEditCelestialViewModel() : base(-1)
        {

        }

        public AddEditCelestialViewModel(long templateId) : base(templateId)
        {
            //apply template
            if (DataTemplate == null)
            {
                //set defaults
            }
            else
            {
            }
        }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Name", Description = "The name of this celestial body.")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [Display(Name = "Orbital Orientation", Description = "What type of orbit this has. Heliocentric means the world orbits this.")]
        [DataType(DataType.Text)]
        public short OrientationType { get; set; }

        [Display(Name = "Apogee", Description = "Maximal distance this is from the world it orbits. (eliptical orbits only, this is averaged with Perigree for circular orbits)")]
        [DataType(DataType.Text)]
        public int Apogee { get; set; }

        [Display(Name = "Perigree", Description = "Minimal distance this is from the world it orbits. (eliptical orbits only, this is averaged with Perigree for circular orbits)")]
        [DataType(DataType.Text)]
        public int Perigree { get; set; }

        [Display(Name = "Velocity", Description = "How fast is this hurtling through space. (affects a LOT of things)")]
        [DataType(DataType.Text)]
        public int Velocity { get; set; }

        [Display(Name = "Luminosity", Description = "How bright is this. Measured in thousands. Anything less than 1000 is not visible to the naked eye.")]
        [DataType(DataType.Text)]
        public int Luminosity { get; set; }

        [StringLength(2000, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 20)]
        [DataType("Markdown")]
        [Display(Name = "Help Text", Description = "The description shown when the Help command is used against this.")]
        public string HelpText { get; set; }

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

        [Display(Name = "Dimensional Model", Description = "The Model used for this")]
        public long DimensionalModelId { get; set; }

        [Display(Name = "Part Name", Description = "The name of the part.")]
        public string[] ModelPartNames { get; set; }

        [Display(Name = "Material", Description = "The material this part is made of.")]
        public long[] ModelPartMaterials { get; set; }

        public IEnumerable<IDimensionalModelData> ValidModels { get; set; }
        public IEnumerable<IMaterial> ValidMaterials { get; set; }
        public IDimensionalModel ModelDataObject { get; set; }

        public ICelestial DataObject { get; set; }
    }
}