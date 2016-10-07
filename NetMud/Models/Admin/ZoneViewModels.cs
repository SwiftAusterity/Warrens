using NetMud.Authentication;
using NetMud.Data.LookupData;
using NetMud.DataStructure.Base.Place;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace NetMud.Models.Admin
{
    public class ManageZoneDataViewModel : PagedDataModel<Zone>, BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageZoneDataViewModel(IEnumerable<Zone> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<Zone, bool> SearchFilter
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

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        
        [Display(Name = "Name")]
        public string Name { get; set; }

        
        [Display(Name = "Claimable")]
        public bool Claimable { get; set; }

        [Range(-5000, 5000, ErrorMessage = "The {0} must be between {2} and {1}.")]
        
        [Display(Name = "Base Elevation")]
        public int BaseElevation { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        
        [Display(Name = "Temperature Coefficient")]
        public int TemperatureCoefficient { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        
        [Display(Name = "Pressure Coefficient")]
        public int PressureCoefficient { get; set; }

        
        [Display(Name = "Base Owner")]
        public long Owner { get; set; }

        [StringLength(2000, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Help Text Body")]
        public string NewHelpBody { get; set; }

        public IZone DataObject { get; set; }
    }
}