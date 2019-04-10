using NetMud.Authentication;
using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataStructure.Room;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.Admin
{
    public class ManageRoomTemplateViewModel : PagedDataModel<IRoomTemplate>
    {
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

        internal override Func<IRoomTemplate, object> OrderPrimary
        {
            get
            {
                return item => item.ParentLocation.Name;
            }
        }


        internal override Func<IRoomTemplate, object> OrderSecondary
        {
            get
            {
                return item => item.Name;
            }
        }
    }

    public class AddEditRoomTemplateViewModel : IBaseViewModel
    {
        public ApplicationUser AuthedUser { get; set; }

        public AddEditRoomTemplateViewModel()
        {
        }

        public IEnumerable<ILocaleTemplate> ValidLocales { get; set; }
        public IEnumerable<IRoomTemplate> ValidRooms { get; set; }
        public IEnumerable<IRoomTemplate> ValidLocaleRooms { get; set; }
        public IEnumerable<IZoneTemplate> ValidZones { get; set; }
        public IEnumerable<IMaterial> ValidMaterials { get; set; }
        public IEnumerable<IDimensionalModelData> ValidModels { get; set; }

        [UIHint("RoomToZonePathwayTemplate")]
        public IPathwayTemplate ZonePathway { get; set; }

        [UIHint("RoomToLocalePathwayTemplate")]
        public IPathwayTemplate LocaleRoomPathway { get; set; }

        [UIHint("LocaleTemplateList")]
        [Display(Name = "Destination Locale", Description = "What locale this path should lead to.")]
        [LocaleTemplateDataBinder]
        public ILocaleTemplate LocaleRoomPathwayDestinationLocale { get; set; }

        [UIHint("RoomTemplateList")]
        [Display(Name = "Destination Room", Description = "What room this path should lead to in the locale.")]
        [RoomTemplateDataBinder]
        public IRoomTemplate LocaleRoomPathwayDestination { get; set; }

        public IRoomTemplate DataObject { get; set; }
    }

    public class RoomMapViewModel : IBaseViewModel
    {
        public ApplicationUser AuthedUser { get; set; }

        public RoomMapViewModel()
        {
        }

        public IRoomTemplate Here { get; set; }
    }
}