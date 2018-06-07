using NetMud.Authentication;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.Supporting;
using NetMud.Models.Admin.PartialInterfaces;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace NetMud.Models.Admin
{
    public class AddEditPathwayDataViewModel : MultipleOccurrenceViewModel, BaseViewModel, ITwoDimensionalEntityViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditPathwayDataViewModel()
        {
            ValidModels = Enumerable.Empty<IDimensionalModelData>();
            ValidMaterials = Enumerable.Empty<IMaterial>();
            ValidRooms = Enumerable.Empty<IRoomData>();
        }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Name")]
        [DataType(DataType.Text)]
        public string Name { get; set; }
        
        [Display(Name = "To Room")]
        [DataType(DataType.Text)]
        public long DestinationID { get; set; }

        [Display(Name = "From Room")]
        [DataType(DataType.Text)]
        public long OriginID { get; set; }

        [Range(-1, 360, ErrorMessage = "The {0} must be between {2} and {1}. -1 is for non-cardinal exits.")]
        [Display(Name = "Degrees From North")]
        [DataType(DataType.Text)]
        public int DegreesFromNorth { get; set; }

        [Display(Name = "Dimensional Model")]
        public long DimensionalModelId { get; set; }

        [Display(Name = "Part Name")]
        public string[] ModelPartNames { get; set; }

        [Display(Name = "Material")]
        public long[] ModelPartMaterials { get; set; }

        [Range(1, 1200, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Length (in)")]
        [DataType(DataType.Text)]
        public int DimensionalModelLength { get; set; }

        [Range(1, 1200, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Height (in)")]
        [DataType(DataType.Text)]
        public int DimensionalModelHeight { get; set; }

        [Range(1, 1200, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Width (in)")]
        [DataType(DataType.Text)]
        public int DimensionalModelWidth { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Hollowness")]
        [DataType(DataType.Text)]
        public int DimensionalModelVacuity { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Surface Cavitation")]
        [DataType(DataType.Text)]
        public int DimensionalModelCavitation { get; set; }

        public IEnumerable<IDimensionalModelData> ValidModels { get; set; }
        public IEnumerable<IMaterial> ValidMaterials { get; set; }
        public IDimensionalModel ModelDataObject { get; set; }

        public IEnumerable<IRoomData> ValidRooms { get; set; }
        public IRoomData Origin { get; set; }
        public IRoomData Destination { get; set; }
        public IPathwayData DataObject { get; set; }
    }

    public class AddPathwayWithRoomDataViewModel : MultipleOccurrenceViewModel, BaseViewModel, ITwoDimensionalEntityViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddPathwayWithRoomDataViewModel()
        {
            ValidModels = Enumerable.Empty<IDimensionalModelData>();
            ValidMaterials = Enumerable.Empty<IMaterial>();
        }

        /* Pathway stuff */
        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Name")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [Display(Name = "From Room")]
        [DataType(DataType.Text)]
        public long OriginID { get; set; }

        [Range(-1, 360, ErrorMessage = "The {0} must be between {2} and {1}. -1 is for non-cardinal exits.")]
        [Display(Name = "Degrees From North")]
        [DataType(DataType.Text)]
        public int DegreesFromNorth { get; set; }

        /* Room stuff */
        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Name")]
        [DataType(DataType.Text)]
        public string RoomName { get; set; }

        [Display(Name = "Medium")]
        [DataType(DataType.Text)]
        public long Medium { get; set; }

        [Display(Name = "Locale")]
        public ILocaleData Locale { get; set; }

        [Range(1, 1200, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Length (inches)")]
        [DataType(DataType.Text)]
        public int RoomDimensionalModelLength { get; set; }

        [Range(1, 1200, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Height (inches)")]
        [DataType(DataType.Text)]
        public int RoomDimensionalModelHeight { get; set; }

        [Range(1, 1200, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Width (inches)")]
        [DataType(DataType.Text)]
        public int RoomDimensionalModelWidth { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Hollowness")]
        [DataType(DataType.Text)]
        public int RoomDimensionalModelVacuity { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Surface Cavitation")]
        [DataType(DataType.Text)]
        public int RoomDimensionalModelCavitation { get; set; }

        [Display(Name = "Create Reciprocal Pathway")]
        public bool CreateReciprocalPath { get; set; }

        public long LocaleId { get; set; }

        [Display(Name = "Dimensional Model")]
        public long DimensionalModelId { get; set; }

        [Display(Name = "Part Name")]
        public string[] ModelPartNames { get; set; }

        [Display(Name = "Material")]
        public long[] ModelPartMaterials { get; set; }

        [Range(1, 1200, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Length (in)")]
        [DataType(DataType.Text)]
        public int DimensionalModelLength { get; set; }

        [Range(1, 1200, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Height (in)")]
        [DataType(DataType.Text)]
        public int DimensionalModelHeight { get; set; }

        [Range(1, 1200, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Width (in)")]
        [DataType(DataType.Text)]
        public int DimensionalModelWidth { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Hollowness")]
        [DataType(DataType.Text)]
        public int DimensionalModelVacuity { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Surface Cavitation")]
        [DataType(DataType.Text)]
        public int DimensionalModelCavitation { get; set; }

        public IEnumerable<IDimensionalModelData> ValidModels { get; set; }
        public IEnumerable<IMaterial> ValidMaterials { get; set; }
        public IDimensionalModel ModelDataObject { get; set; }

        public IEnumerable<IRoomData> ValidRooms { get; set; }
        public IRoomData Origin { get; set; }
        public IPathwayData DataObject { get; set; }
    }
}