using NetMud.Authentication;
using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Room;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace NetMud.Models.Admin
{
    public class AddEditPathwayTemplateViewModel : IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditPathwayTemplateViewModel()
        {
            ValidModels = Enumerable.Empty<IDimensionalModelData>();
            ValidMaterials = Enumerable.Empty<IMaterial>();
            ValidRooms = Enumerable.Empty<IRoomTemplate>();
        }

        public IEnumerable<IRoomTemplate> ValidRooms { get; set; }
        public IPathwayTemplate DataObject { get; set; }
        public IEnumerable<IDimensionalModelData> ValidModels { get; set; }
        public IEnumerable<IMaterial> ValidMaterials { get; set; }
    }

    public class AddPathwayWithRoomTemplateViewModel : IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddPathwayWithRoomTemplateViewModel()
        {
            ValidModels = Enumerable.Empty<IDimensionalModelData>();
            ValidMaterials = Enumerable.Empty<IMaterial>();
        }

        [Display(Name = "Create Reciprocal Pathway", Description = "Should an identical pathway be created leading in the opposite direction. (likely TRUE)")]
        [UIHint("Boolean")]
        public bool CreateReciprocalPath { get; set; }

        public IEnumerable<IRoomTemplate> ValidRooms { get; set; }

        [Display(Name = "Origin", Description = "The room this starts from")]
        [UIHint("RoomTemplateList")]
        [RoomTemplateDataBinder]
        public IRoomTemplate Origin { get; set; }

        public IRoomTemplate Destination { get; set; }
        public IPathwayTemplate DataObject { get; set; }
        public IEnumerable<IDimensionalModelData> ValidModels { get; set; }
        public IEnumerable<IMaterial> ValidMaterials { get; set; }
    }
}