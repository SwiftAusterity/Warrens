using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.Supporting;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace NetMud.Models.Admin
{
    public class AddEditPathwayDataViewModel : TwoDimensionalEntityEditViewModel, BaseViewModel
    {
        public AddEditPathwayDataViewModel()
        {
            ValidModels = Enumerable.Empty<IDimensionalModelData>();
            ValidMaterials = Enumerable.Empty<IMaterial>();
            ValidRooms = Enumerable.Empty<IRoomData>();
        }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Name", Description = "The descriptive name of this path. Used to identify it when Descriptives lack.")]
        [DataType(DataType.Text)]
        public string Name { get; set; }
        
        [Display(Name = "To Room", Description = "The room this leads to.")]
        [DataType(DataType.Text)]
        public long DestinationID { get; set; }

        [Display(Name = "From Room", Description = "The room this originates from.")]
        [DataType(DataType.Text)]
        public long OriginID { get; set; }

        [Range(-1, 360, ErrorMessage = "The {0} must be between {2} and {1}. -1 is for non-cardinal exits.")]
        [Display(Name = "Degrees From North", Description = "The direction on a 360 plane. 360 and 0 are both directional north. 90 is east, 180 is south, 270 is west.")]
        [DataType(DataType.Text)]
        public int DegreesFromNorth { get; set; }

        public IEnumerable<IRoomData> ValidRooms { get; set; }
        public IRoomData Origin { get; set; }
        public IRoomData Destination { get; set; }
        public IPathwayData DataObject { get; set; }
    }

    public class AddPathwayWithRoomDataViewModel : TwoDimensionalEntityEditViewModel, BaseViewModel
    {
        public AddPathwayWithRoomDataViewModel()
        {
            ValidModels = Enumerable.Empty<IDimensionalModelData>();
            ValidMaterials = Enumerable.Empty<IMaterial>();
        }

        /* Pathway stuff */
        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Name", Description = "The descriptive name of this path. Used to identify it when Descriptives lack.")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [Display(Name = "From Room", Description = "The room this originates from.")]
        [DataType(DataType.Text)]
        public long OriginID { get; set; }

        [Range(-1, 360, ErrorMessage = "The {0} must be between {2} and {1}. -1 is for non-cardinal exits.")]
        [Display(Name = "Degrees From North", Description = "The direction on a 360 plane. 360 and 0 are both directional north. 90 is east, 180 is south, 270 is west.")]
        [DataType(DataType.Text)]
        public int DegreesFromNorth { get; set; }

        /* Room stuff */
        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Name", Description = "The room's descriptive name.")]
        [DataType(DataType.Text)]
        public string RoomName { get; set; }

        [Display(Name = "Medium", Description = "What the 'empty' space of the room is made of. (likely AIR, sometimes stone or dirt)")]
        [DataType(DataType.Text)]
        public long Medium { get; set; }

        [Display(Name = "Locale", Description = "The locale this belongs to.")]
        public ILocaleData Locale { get; set; }

        [Range(1, 1200, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Length (inches)", Description = "The dimensional length of the pathway.")]
        [DataType(DataType.Text)]
        public int RoomDimensionalModelLength { get; set; }

        [Range(1, 1200, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Height (inches)", Description = "The dimensional height of the pathway.")]
        [DataType(DataType.Text)]
        public int RoomDimensionalModelHeight { get; set; }

        [Range(1, 1200, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Width (inches)", Description = "The dimensional width of the pathway.")]
        [DataType(DataType.Text)]
        public int RoomDimensionalModelWidth { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Hollowness", Description = "The hollowness of the pathway that leads out of this to a zone. Very hollow paths can be crawled through by smaller creatures.")]
        [DataType(DataType.Text)]
        public int RoomDimensionalModelVacuity { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Surface Cavitation", Description = "The cavitation of the surface of the pathway that leads out of this to a zone. High cavitation + high hollowness = passthru.")]
        [DataType(DataType.Text)]
        public int RoomDimensionalModelCavitation { get; set; }

        [Display(Name = "Create Reciprocal Pathway", Description = "Should an identical pathway be created leading in the opposite direction. (likely TRUE)")]
        public bool CreateReciprocalPath { get; set; }

        public long LocaleId { get; set; }

        public IEnumerable<IRoomData> ValidRooms { get; set; }
        public IRoomData Origin { get; set; }
        public IPathwayData DataObject { get; set; }
    }
}