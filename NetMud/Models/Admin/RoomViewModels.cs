using NetMud.Authentication;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.Supporting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.Admin
{
    public class ManageRoomDataViewModel : PagedDataModel<IRoomData>, BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageRoomDataViewModel(IEnumerable<IRoomData> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IRoomData, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }
    }

    public class AddEditRoomDataViewModel : DimensionalEntityEditViewModel
    {
        public AddEditRoomDataViewModel()
        {
        }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Name")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [Display(Name = "Medium")]
        [DataType(DataType.Text)]
        public long Medium { get; set; }

        [Display(Name = "Locale")]
        public ILocaleData Locale { get; set; }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Name")]
        [DataType(DataType.Text)]
        public string ZonePathwayName { get; set; }

        [DataType(DataType.Text)]
        public long ZoneDestinationId { get; set; }

        public IEnumerable<IMaterial> ValidMaterials { get; set; }

        [Range(1, 1200, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Length (inches)")]
        [DataType(DataType.Text)]
        public int ZoneDimensionalModelLength { get; set; }

        [Range(1, 1200, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Height (inches)")]
        [DataType(DataType.Text)]
        public int ZoneDimensionalModelHeight { get; set; }

        [Range(1, 1200, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Width (inches)")]
        [DataType(DataType.Text)]
        public int ZoneDimensionalModelWidth { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Hollowness")]
        [DataType(DataType.Text)]
        public int ZoneDimensionalModelVacuity { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Surface Cavitation")]
        [DataType(DataType.Text)]
        public int ZoneDimensionalModelCavitation { get; set; }

        public IEnumerable<IZoneData> ValidZones { get; set; }

        public IPathwayData ZonePathway { get; set; }

        public IRoomData DataObject { get; set; }
    }

    public class RoomMapViewModel : BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public RoomMapViewModel()
        {
        }

        public IRoomData Here { get; set; }
    }
}