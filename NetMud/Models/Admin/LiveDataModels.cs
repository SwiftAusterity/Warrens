using NetMud.Authentication;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Room;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;


namespace NetMud.Models.Admin
{
    public abstract class LiveEntityViewModel : IBaseViewModel
    {
        public ApplicationUser AuthedUser { get; set; }
    }

    public class LiveRoomsViewModel : PagedDataModel<IRoom>
    {
        public LiveRoomsViewModel(IEnumerable<IRoom> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
            ValidEntities = Enumerable.Empty<IRoom>();
        }

        internal override Func<IRoom, object> OrderPrimary
        {
            get
            {
                return item => item.TemplateName;
            }
        }


        internal override Func<IRoom, object> OrderSecondary
        {
            get
            {
                return null;
            }
        }

        internal override Func<IRoom, bool> SearchFilter
        {
            get
            {
                return item => SearchTerms.ToLower().Contains(item.TemplateName);
            }
        }

        public IEnumerable<IRoom> ValidEntities { get; set; }
    }

    public class ViewRoomViewModel : LiveEntityViewModel
    {
        public ViewRoomViewModel()
        {
        }

        public ViewRoomViewModel(string birthMark)
        {
            DataObject = LiveCache.Get<IRoom>(new LiveCacheKey(typeof(IRoom), birthMark));
        }

        [UIHint("Room")]
        public IRoom DataObject { get; set; }
    }
}