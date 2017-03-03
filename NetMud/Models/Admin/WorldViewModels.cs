using NetMud.Authentication;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.Supporting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace NetMud.Models.Admin
{
    public class ManageWorldViewModel : PagedDataModel<IWorld>, BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageWorldViewModel(IEnumerable<IWorld> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IWorld, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }
    }

    public class AddEditWorldViewModel : BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditWorldViewModel()
        {
            ValidMaterials = Enumerable.Empty<IMaterial>();
        }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Name")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [Range(1, 999999999999999, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Full Diameter")]
        [DataType(DataType.Text)]
        public long FullDiameter { get; set; }

        [Display(Name = "Topography")]
        [UIHint("EnumDropDownList")]
        public WorldType Topography { get; set; }

        public IEnumerable<IMaterial> ValidMaterials { get; set; }

        public IWorld DataObject { get; set; }
    }

    public class AddEditStratumViewModel : BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditStratumViewModel(IStratum stratum, IEnumerable<IMaterial> validMaterials)
        {
            DataObject = stratum;
            ValidMaterials = validMaterials;
        }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Name")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [Range(1, 100000, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Diameter")]
        [DataType(DataType.Text)]
        public long Diameter { get; set; }
        
        [Range(-100, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Ambient Temperature Low")]
        [DataType(DataType.Text)]
        public int AmbientTemperatureRangeLow { get; set; }

        [Range(-100, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Ambient Temperature High")]
        [DataType(DataType.Text)]
        public int AmbientTemperatureRangeHigh { get; set; }
        
        [Range(-100, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Ambient Humidity Low")]
        [DataType(DataType.Text)]
        public int AmbientHumidityRangeLow { get; set; }

        [Range(-100, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Ambient Humidity High")]
        [DataType(DataType.Text)]
        public int AmbientHumidityRangeHigh { get; set; }

        [Display(Name = "Layer Material")]
        public long[] LayerMaterials { get; set; }

        [Display(Name = "Lower Depth")]
        public int[] LowerDepths { get; set; }

        [Display(Name = "Upper Depth")]
        public int[] UpperDepths { get; set; }

        public IEnumerable<IMaterial> ValidMaterials { get; set; }

        public IStratum DataObject { get; set; }
    }
}