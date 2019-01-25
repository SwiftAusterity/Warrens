using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Room;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace NetMud.Models.Admin
{
    public class AddEditPathwayTemplateViewModel : TwoDimensionalEntityEditViewModel, IBaseViewModel
    {
        public AddEditPathwayTemplateViewModel()
        {
            ValidModels = Enumerable.Empty<IDimensionalModelData>();
            ValidMaterials = Enumerable.Empty<IMaterial>();
            ValidRooms = Enumerable.Empty<IRoomTemplate>();
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

        public IEnumerable<IRoomTemplate> ValidRooms { get; set; }
        public IRoomTemplate Origin { get; set; }
        public IRoomTemplate Destination { get; set; }
        public IPathwayTemplate DataObject { get; set; }
    }

    public class AddPathwayWithRoomTemplateViewModel : TwoDimensionalEntityEditViewModel, IBaseViewModel
    {
        public AddPathwayWithRoomTemplateViewModel()
        {
            ValidModels = Enumerable.Empty<IDimensionalModelData>();
            ValidMaterials = Enumerable.Empty<IMaterial>();
        }

        [Display(Name = "Create Reciprocal Pathway", Description = "Should an identical pathway be created leading in the opposite direction. (likely TRUE)")]
        [UIHint("Boolean")]
        public bool CreateReciprocalPath { get; set; }

        [Display(Name = "From Room", Description = "The room this originates from.")]
        [DataType(DataType.Text)]
        public long OriginID { get; set; }

        public IEnumerable<IRoomTemplate> ValidRooms { get; set; }
        public IRoomTemplate Origin { get; set; }
        public IRoomTemplate Destination { get; set; }
        public IPathwayTemplate DataObject { get; set; }
    }
}