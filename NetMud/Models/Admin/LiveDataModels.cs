using NetMud.Authentication;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Gaia;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.Locale;
using NetMud.DataStructure.NPC;
using NetMud.DataStructure.Room;
using NetMud.DataStructure.Zone;
using System;
using System.Collections.Generic;
using System.Linq;


namespace NetMud.Models.Admin
{
    public class LiveWorldsViewModel : PagedDataModel<IGaia>
    {
        public LiveWorldsViewModel(IEnumerable<IGaia> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
            ValidEntities = Enumerable.Empty<IGaia>();
        }

        internal override Func<IGaia, bool> SearchFilter
        {
            get
            {
                return item => SearchTerms.ToLower().Contains(item.TemplateName);
            }
        }


        internal override Func<IGaia, object> OrderPrimary
        {
            get
            {
                return item => item.TemplateName;
            }
        }

        internal override Func<IGaia, object> OrderSecondary
        {
            get
            {
                return null;
            }
        }

        public IEnumerable<IGaia> ValidEntities { get; set; }
    }

    public class ViewGaiaViewModel : IBaseViewModel
    {
        public ApplicationUser AuthedUser { get; set; }

        public ViewGaiaViewModel()
        {
        }

        public ViewGaiaViewModel(string birthMark)
        {
            DataObject = LiveCache.Get<IGaia>(new LiveCacheKey(typeof(IGaia), birthMark));
        }

        public IGaia DataObject { get; set; }
    }

    public class LiveZonesViewModel : PagedDataModel<IZone>
    {
        public LiveZonesViewModel(IEnumerable<IZone> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
            ValidEntities = Enumerable.Empty<IZone>();
        }

        internal override Func<IZone, bool> SearchFilter
        {
            get
            {
                return item => SearchTerms.ToLower().Contains(item.TemplateName);
            }
        }


        internal override Func<IZone, object> OrderPrimary
        {
            get
            {
                return item => item.TemplateName;
            }
        }


        internal override Func<IZone, object> OrderSecondary
        {
            get
            {
                return null;
            }
        }

        public IEnumerable<IZone> ValidEntities { get; set; }
    }

    public class ViewZoneViewModel : IBaseViewModel
    {
        public ApplicationUser AuthedUser { get; set; }

        public ViewZoneViewModel()
        {
        }

        public ViewZoneViewModel(string birthMark)
        {
            DataObject = LiveCache.Get<IZone>(new LiveCacheKey(typeof(IZone), birthMark));
        }

        public IZone DataObject { get; set; }
    }

    public class LiveInanimatesViewModel : PagedDataModel<IInanimate>
    {
        public LiveInanimatesViewModel(IEnumerable<IInanimate> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
            ValidEntities = Enumerable.Empty<IInanimate>();
        }

        internal override Func<IInanimate, object> OrderPrimary
        {
            get
            {
                return item => item.TemplateName;
            }
        }


        internal override Func<IInanimate, object> OrderSecondary
        {
            get
            {
                return null;
            }
        }

        internal override Func<IInanimate, bool> SearchFilter
        {
            get
            {
                return item => SearchTerms.ToLower().Contains(item.TemplateName);
            }
        }

        public IEnumerable<IInanimate> ValidEntities { get; set; }
    }

    public class ViewInanimateViewModel : IBaseViewModel
    {
        public ApplicationUser AuthedUser { get; set; }

        public ViewInanimateViewModel()
        {
        }

        public ViewInanimateViewModel(string birthMark)
        {
            DataObject = LiveCache.Get<IInanimate>(new LiveCacheKey(typeof(IInanimate), birthMark));
        }

        public IInanimate DataObject { get; set; }
    }

    public class LiveNPCsViewModel : PagedDataModel<INonPlayerCharacter>
    {
        public LiveNPCsViewModel(IEnumerable<INonPlayerCharacter> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
            ValidEntities = Enumerable.Empty<INonPlayerCharacter>();
        }

        internal override Func<INonPlayerCharacter, object> OrderPrimary
        {
            get
            {
                return item => item.TemplateName;
            }
        }


        internal override Func<INonPlayerCharacter, object> OrderSecondary
        {
            get
            {
                return null;
            }
        }

        internal override Func<INonPlayerCharacter, bool> SearchFilter
        {
            get
            {
                return item => SearchTerms.ToLower().Contains(item.TemplateName);
            }
        }

        public IEnumerable<INonPlayerCharacter> ValidEntities { get; set; }
    }

    public class ViewIntelligenceViewModel : IBaseViewModel
    {
        public ApplicationUser AuthedUser { get; set; }

        public ViewIntelligenceViewModel()
        {
        }

        public ViewIntelligenceViewModel(string birthMark)
        {
            DataObject = LiveCache.Get<INonPlayerCharacter>(new LiveCacheKey(typeof(INonPlayerCharacter), birthMark));
        }

        public INonPlayerCharacter DataObject { get; set; }
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

    public class ViewRoomViewModel : IBaseViewModel
    {
        public ApplicationUser AuthedUser { get; set; }

        public ViewRoomViewModel()
        {
        }

        public ViewRoomViewModel(string birthMark)
        {
            DataObject = LiveCache.Get<IRoom>(new LiveCacheKey(typeof(IRoom), birthMark));
        }

        public IRoom DataObject { get; set; }
    }

    public class LiveLocalesViewModel : PagedDataModel<ILocale>
    {
        public LiveLocalesViewModel(IEnumerable<ILocale> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
            ValidEntities = Enumerable.Empty<ILocale>();
        }

        internal override Func<ILocale, object> OrderPrimary
        {
            get
            {
                return item => item.TemplateName;
            }
        }


        internal override Func<ILocale, object> OrderSecondary
        {
            get
            {
                return null;
            }
        }

        internal override Func<ILocale, bool> SearchFilter
        {
            get
            {
                return item => SearchTerms.ToLower().Contains(item.TemplateName);
            }
        }

        public IEnumerable<ILocale> ValidEntities { get; set; }
    }

    public class ViewLocaleViewModel : IBaseViewModel
    {
        public ApplicationUser AuthedUser { get; set; }

        public ViewLocaleViewModel()
        {
        }

        public ViewLocaleViewModel(string birthMark)
        {
            DataObject = LiveCache.Get<ILocale>(new LiveCacheKey(typeof(ILocale), birthMark));
        }

        public ILocale DataObject { get; set; }
    }
}