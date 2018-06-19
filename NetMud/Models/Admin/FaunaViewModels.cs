using NetMud.Authentication;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Supporting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;


namespace NetMud.Models.Admin
{
    public class ManageFaunaViewModel : PagedDataModel<IFauna>, BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageFaunaViewModel(IEnumerable<IFauna> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IFauna, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }
    }

    public class AddEditFaunaViewModel : BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditFaunaViewModel()
        {
            ValidInanimateDatas = Enumerable.Empty<IInanimateData>();
            ValidMaterials = Enumerable.Empty<IMaterial>();
            ValidRaces = Enumerable.Empty<IRace>();
        }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Name", Description = "The keyword name used for this.")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [StringLength(2000, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 20)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Help Text", Description = "The description shown when the Help command is used against this.")]
        public string HelpText { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Spawn Multiplier")]
        [DataType(DataType.Text)]
        public int AmountMultiplier { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Spawn Rarity")]
        [DataType(DataType.Text)]
        public int Rarity { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Puissance Variance")]
        [DataType(DataType.Text)]
        public int PuissanceVariance { get; set; }

        [Range(-1000, 1000, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Y-Axis High")]
        [DataType(DataType.Text)]
        public int ElevationRangeHigh { get; set; }

        [Range(-2000, 2000, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Y-Axis Low")]
        [DataType(DataType.Text)]
        public int ElevationRangeLow { get; set; }

        [Range(-2000, 2000, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Temperature High")]
        [DataType(DataType.Text)]
        public int TemperatureRangeHigh { get; set; }

        [Range(-2000, 2000, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Temperature Low")]
        [DataType(DataType.Text)]
        public int TemperatureRangeLow { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Humidity High")]
        [DataType(DataType.Text)]
        public int HumidityRangeHigh { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Humidity Low")]
        [DataType(DataType.Text)]
        public int HumidityRangeLow { get; set; }

        [Display(Name = "Occurs in Biome")]
        public Biome[] OccursIn { get; set; }

        [Range(1, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Ratio Female to Male")]
        [DataType(DataType.Text)]
        public int FemaleRatio { get; set; }

        [Range(1, 2000, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Total Pop Cap")]
        [DataType(DataType.Text)]
        public int PopulationHardCap { get; set; }

        [Display(Name = "Race")]
        public long Race { get; set; }

        public IEnumerable<IRace> ValidRaces { get; set; }
        public IEnumerable<IInanimateData> ValidInanimateDatas { get; set; }
        public IEnumerable<IMaterial> ValidMaterials { get; set; }
        public IFauna DataObject { get; set; }
    }
}