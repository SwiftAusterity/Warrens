using NetMud.Authentication;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Supporting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;


namespace NetMud.Models.Admin
{
    public class ManageFloraViewModel : PagedDataModel<IFlora>, BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageFloraViewModel(IEnumerable<IFlora> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IFlora, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }
    }

    public class AddEditFloraViewModel : BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditFloraViewModel()
        {
            ValidInanimateDatas = Enumerable.Empty<IInanimateData>();
            ValidMaterials = Enumerable.Empty<IMaterial>();
        }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Name", Description = "The descriptive name used to refer to this.")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Sunlight Preference", Description = "What level of sunlight does this prefer.")]
        [DataType(DataType.Text)]
        public int SunlightPreference { get; set; }

        [Display(Name = "Coniferous", Description = "Does this continue to grow in the winter.")]
        public bool Coniferous { get; set; }

        [StringLength(2000, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 20)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Help Text", Description = "The descriptive text shown on the list page and in the help system for this.")]
        public string HelpText { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Spawn Multiplier", Description = "The factor that governs how much of this spawns in a new location.")]
        [DataType(DataType.Text)]
        public int AmountMultiplier { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Spawn Rarity", Description = "How rare is this to spawn at all in a new location.")]
        [DataType(DataType.Text)]
        public int Rarity { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Puissance Variance", Description = "How much deviation in random magical strength will be spawned in.")]
        [DataType(DataType.Text)]
        public int PuissanceVariance { get; set; }

        [Range(-1000, 1000, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Y-Axis High", Description = "The upper elevation cap this will allow to spawn in.")]
        [DataType(DataType.Text)]
        public int ElevationRangeHigh { get; set; }

        [Range(-2000, 2000, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Y-Axis Low", Description = "The lower elevation cap this will allow to spawn in.")]
        [DataType(DataType.Text)]
        public int ElevationRangeLow { get; set; }

        [Range(-2000, 2000, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Temperature High", Description = "The upper temperature cap this will allow to spawn in.")]
        [DataType(DataType.Text)]
        public int TemperatureRangeHigh { get; set; }

        [Range(-2000, 2000, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Temperature Low", Description = "The lower temperature cap this will allow to spawn in.")]
        [DataType(DataType.Text)]
        public int TemperatureRangeLow { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Humidity High", Description = "The upper barometric pressure cap this will allow to spawn in.")]
        [DataType(DataType.Text)]
        public int HumidityRangeHigh { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Humidity Low", Description = "The lower barometric pressure cap this will allow to spawn in.")]
        [DataType(DataType.Text)]
        public int HumidityRangeLow { get; set; }

        [Display(Name = "Occurs in Biome", Description = "What biomes this will allow to spawn in.")]
        public Biome[] OccursIn { get; set; }

        [Display(Name = "Wood/Bark")]
        public long Wood { get; set; }

        [Display(Name = "Flower")]
        public long Flower { get; set; }

        [Display(Name = "Leaves")]
        public long Leaf { get; set; }

        [Display(Name = "Fruit")]
        public long Fruit { get; set; }

        [Display(Name = "Seed")]
        public long Seed { get; set; }

        public IEnumerable<IInanimateData> ValidInanimateDatas { get; set; }
        public IEnumerable<IMaterial> ValidMaterials { get; set; }
        public IFlora DataObject { get; set; }
    }
}