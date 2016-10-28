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
        public string NewName { get; set; }

        [Display(Name = "Medium")]
        [DataType(DataType.Text)]
        public long Medium { get; set; }

        [Display(Name = "Zone")]
        [DataType(DataType.Text)]
        public long Zone { get; set; }

        [Display(Name = "Border")]
        public string[] BorderNames { get; set; }

        [Display(Name = "Material")]
        public long[] BorderMaterials { get; set; }

        public IEnumerable<IMaterial> ValidMaterials { get; set; }

        public IEnumerable<IZone> ValidZones { get; set; }

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