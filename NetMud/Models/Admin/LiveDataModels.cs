using NetMud.Authentication;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Gaia;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.NPC;
using NetMud.DataStructure.Zone;
using System;
using System.Collections.Generic;
using System.Linq;


namespace NetMud.Models.Admin
{
    public class LiveWorldsViewModel : PagedDataModel<IGaia>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

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

        public IEnumerable<IGaia> ValidEntities { get; set; }
    }

    public class ViewGaiaViewModel : IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ViewGaiaViewModel()
        {
        }

        public ViewGaiaViewModel(string birthMark)
        {
            DataObject = LiveCache.Get<IGaia>(new LiveCacheKey(typeof(IGaia), birthMark));
        }

        public IGaia DataObject { get; set; }
    }

    public class LiveZonesViewModel : PagedDataModel<IZone>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

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

        public IEnumerable<IZone> ValidEntities { get; set; }
    }

    public class ViewZoneViewModel : IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ViewZoneViewModel()
        {
        }

        public ViewZoneViewModel(string birthMark)
        {
            DataObject = LiveCache.Get<IZone>(new LiveCacheKey(typeof(IZone), birthMark));
        }

        public IZone DataObject { get; set; }
    }

    public class LiveInanimatesViewModel : PagedDataModel<IInanimate>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public LiveInanimatesViewModel(IEnumerable<IInanimate> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
            ValidEntities = Enumerable.Empty<IInanimate>();
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
        public ApplicationUser authedUser { get; set; }

        public ViewInanimateViewModel()
        {
        }

        public ViewInanimateViewModel(string birthMark)
        {
            DataObject = LiveCache.Get<IInanimate>(new LiveCacheKey(typeof(IInanimate), birthMark));
        }

        public IInanimate DataObject { get; set; }
    }

    public class LiveNPCsViewModel : PagedDataModel<INonPlayerCharacter>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public LiveNPCsViewModel(IEnumerable<INonPlayerCharacter> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
            ValidEntities = Enumerable.Empty<INonPlayerCharacter>();
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
        public ApplicationUser authedUser { get; set; }

        public ViewIntelligenceViewModel()
        {
        }

        public ViewIntelligenceViewModel(string birthMark)
        {
            DataObject = LiveCache.Get<INonPlayerCharacter>(new LiveCacheKey(typeof(INonPlayerCharacter), birthMark));
        }

        public INonPlayerCharacter DataObject { get; set; }
    }
}