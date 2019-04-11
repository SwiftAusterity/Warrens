using NetMud.Authentication;
using NetMud.DataStructure.Room;
using System;
using System.Collections.Generic;

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
                return item => item.Name;
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

        public IEnumerable<IRoomTemplate> ValidRooms { get; set; }

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