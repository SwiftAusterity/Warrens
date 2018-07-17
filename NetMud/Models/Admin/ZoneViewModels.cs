using NetMud.Authentication;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.World;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

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

    public class AddEditZoneDataViewModel : PagedDataModel<ILocaleData>, BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }


        internal override Func<ILocaleData, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }

        public AddEditZoneDataViewModel(IEnumerable<ILocaleData> items)
        : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        public AddEditZoneDataViewModel() : base(Enumerable.Empty<ILocaleData>())
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }


        [StringLength(100, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Name", Description = "The identifying name of the zone. Displays above the output window.")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [Display(Name = "Claimable", Description = "Can players create locations in this zone (Likely FALSE).")]
        public bool Claimable { get; set; }

        [Range(-5000, 5000, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Base Elevation", Description = "The 'sea level' for this zone. All locales will treat this as Y=0.")]
        [DataType(DataType.Text)]
        public int BaseElevation { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Temperature Coefficient", Description = "The volatility of the temperatures of this zone. Used in the weather system.")]
        [DataType(DataType.Text)]
        public int TemperatureCoefficient { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Pressure Coefficient", Description = "The volitility of humditity in this zone. Used in the weather system.")]
        [DataType(DataType.Text)]
        public int PressureCoefficient { get; set; }

        [Display(Name = "World", Description = "The World/Dimension this belongs to.")]
        [DataType(DataType.Text)]
        public long World { get; set; }

        [Display(Name = "Hemisphere", Description = "The hemisphere of the world this zone is in.")]
        [DataType(DataType.Text)]
        public short Hemisphere { get; set; }


        public IEnumerable<IGaiaData> ValidWorlds { get; set; }
        public IZoneData DataObject { get; set; }
    }

    public class AddEditZonePathwayDataViewModel : TwoDimensionalEntityEditViewModel, BaseViewModel
    {
        public AddEditZonePathwayDataViewModel()
        {
            ValidModels = Enumerable.Empty<IDimensionalModelData>();
            ValidMaterials = Enumerable.Empty<IMaterial>();
            ValidRooms = Enumerable.Empty<IRoomData>();
        }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Name", Description = "The identifying name of the pathway.")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [Display(Name = "To Room", Description = "What room this will send you to.")]
        [DataType(DataType.Text)]
        public long DestinationID { get; set; }

        [Display(Name = "From Zone", Description = "The zone this originates from.")]
        [DataType(DataType.Text)]
        public long OriginID { get; set; }

        public IEnumerable<IRoomData> ValidRooms { get; set; }

        public IZoneData Origin { get; set; }
        public IRoomData Destination { get; set; }
        public IPathwayData DataObject { get; set; }
    }
}