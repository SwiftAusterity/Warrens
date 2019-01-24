using NetMud.Authentication;
using NetMud.DataStructure.Architectural.EntityBase;
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

        public IEnumerable<IZoneTemplate> ValidZones { get; set; }
        public IEnumerable<IMaterial> ValidMaterials { get; set; }
        public IEnumerable<IDimensionalModelData> ValidModels { get; set; }

        [UIHint("RoomToZonePathwayTemplate")]
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