using NetMud.Authentication;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace NetMud.Models.Admin
{
    public class ManageZoneDataViewModel : PagedDataModel<IZoneData>, BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageZoneDataViewModel(IEnumerable<IZoneData> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IZoneData, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }
    }

    public class AddEditZoneDataViewModel : BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditZoneDataViewModel()
        {
        }

        [StringLength(100, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Name")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [Display(Name = "Claimable")]
        public bool Claimable { get; set; }

        [Range(-5000, 5000, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Base Elevation")]
        [DataType(DataType.Text)]
        public int BaseElevation { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Temperature Coefficient")]
        [DataType(DataType.Text)]
        public int TemperatureCoefficient { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Pressure Coefficient")]
        [DataType(DataType.Text)]
        public int PressureCoefficient { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 3)]
        [Display(Name = "World Name")]
        [DataType(DataType.Text)]
        public string WorldName { get; set; }

        [StringLength(2000, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Help Text Body")]
        public string NewHelpBody { get; set; }

        public IZoneData DataObject { get; set; }
    }
}