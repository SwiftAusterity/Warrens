using NetMud.Authentication;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Locale;
using NetMud.DataStructure.Room;
using NetMud.DataStructure.Zone;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.Admin
{
    public class ManageRoomTemplateViewModel : PagedDataModel<IRoomTemplate>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageRoomTemplateViewModel(IEnumerable<IRoomTemplate> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IRoomTemplate, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }
    }

    public class AddEditRoomTemplateViewModel : DimensionalEntityEditViewModel
    {
        public AddEditRoomTemplateViewModel()
        {
        }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Name", Description = "Descriptive name for this room. Displayed above the output window in the client.")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [Display(Name = "Medium", Description = "What the 'empty' space of the room is made of. (likely AIR, sometimes stone or dirt)")]
        [DataType(DataType.Text)]
        public long Medium { get; set; }

        [Display(Name = "Locale", Description = "The locale this belongs to.")]
        public ILocaleTemplate Locale { get; set; }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Name", Description = "The name of the pathway that leads out of this to a zone.")]
        [DataType(DataType.Text)]
        public string ZonePathwayName { get; set; }

        [DataType(DataType.Text)]
        public long ZoneDestinationId { get; set; }

        public IEnumerable<IMaterial> ValidMaterials { get; set; }

        [Range(1, 1200, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Length (inches)", Description = "The dimensional length of the pathway that leads out of this to a zone.")]
        [DataType(DataType.Text)]
        public int ZoneDimensionalModelLength { get; set; }

        [Range(1, 1200, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Height (inches)", Description = "The dimensional height of the pathway that leads out of this to a zone.")]
        [DataType(DataType.Text)]
        public int ZoneDimensionalModelHeight { get; set; }

        [Range(1, 1200, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Width (inches)", Description = "The dimensional width of the pathway that leads out of this to a zone.")]
        [DataType(DataType.Text)]
        public int ZoneDimensionalModelWidth { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Hollowness", Description = "The hollowness of the pathway that leads out of this to a zone. Very hollow paths can be crawled through by smaller creatures.")]
        [DataType(DataType.Text)]
        public int ZoneDimensionalModelVacuity { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Surface Cavitation", Description = "The cavitation of the surface of the pathway that leads out of this to a zone. High cavitation + high hollowness = passthru.")]
        [DataType(DataType.Text)]
        public int ZoneDimensionalModelCavitation { get; set; }

        public IEnumerable<IZoneTemplate> ValidZones { get; set; }

        public IPathwayTemplate ZonePathway { get; set; }

        public IRoomTemplate DataObject { get; set; }
    }

    public class RoomMapViewModel : IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public RoomMapViewModel()
        {
        }

        public IRoomTemplate Here { get; set; }
    }
}