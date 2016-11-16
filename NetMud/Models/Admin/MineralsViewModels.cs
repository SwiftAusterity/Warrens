using NetMud.Authentication;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;


namespace NetMud.Models.Admin
{
    public class ManageMineralsViewModel : PagedDataModel<IMineral>, BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageMineralsViewModel(IEnumerable<IMineral> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IMineral, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }
    }

    public class AddEditMineralsViewModel : BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditMineralsViewModel()
        {
            ValidInanimateDatas = Enumerable.Empty<IInanimateData>();
            ValidMaterials = Enumerable.Empty<IMaterial>();
        }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Name")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [StringLength(2000, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 20)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Help Text")]
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

        [Display(Name = "Occurs in Material")]
        public long[] OccursIn { get; set; }

        [Display(Name = "Rock")]
        public long Rock { get; set; }

        [Display(Name = "Dirt")]
        public long Dirt { get; set; }

        public IEnumerable<IInanimateData> ValidInanimateDatas { get; set; }
        public IEnumerable<IMaterial> ValidMaterials { get; set; }
        public IMineral DataObject { get; set; }
    }
}