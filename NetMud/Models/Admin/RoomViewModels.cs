using Microsoft.AspNet.Identity.EntityFramework;
using NetMud.Authentication;
using NetMud.Data.Reference;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Web;

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
        [DataType(DataType.Text)]
        [Display(Name = "Name")]
        public string NewName { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Medium")]
        public long Medium { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Zone")]
        public long Zone { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Border")]
        public string[] BorderNames { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Material")]
        public long[] BorderMaterials { get; set; }

        public IEnumerable<IMaterial> ValidMaterials { get; set; }

        public IEnumerable<IZone> ValidZones { get; set; }

        public IRoomData DataObject { get; set; }
    }
}