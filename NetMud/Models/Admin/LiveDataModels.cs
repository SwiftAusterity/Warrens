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

        [Display(Name = "Game UI Language", Description = "The language the game will output to you while playing.")]
        [UIHint("LanguageList")]
        [LanguageDataBinder]
        public ILanguage Language { get; set; }

        [Display(Name = "Elegance", Description = "The quality Delta against the current word.")]
        [DataType(DataType.Text)]
        public int Elegance { get; set; }

        [Display(Name = "Severity", Description = "The quality Delta against the current word.")]
        [DataType(DataType.Text)]
        public int Severity { get; set; }

        [Display(Name = "Quality", Description = "The quality Delta against the current word.")]
        [DataType(DataType.Text)]
        public int Quality { get; set; }

        public IEnumerable<ILanguage> ValidLanguages { get; set; }

        public LiveEntityViewModel()
        {
            ValidLanguages = ConfigDataCache.GetAll<ILanguage>();
        }
    }

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

    public class ViewGaiaViewModel : LiveEntityViewModel
    {
        public ViewGaiaViewModel()
        {
            ValidCelestials = TemplateCache.GetAll<ICelestial>(true);
            ValidInanimateTemplates = TemplateCache.GetAll<IInanimateTemplate>(true);
        }

        public ViewGaiaViewModel(string birthMark)
        {
            DataObject = LiveCache.Get<IGaia>(new LiveCacheKey(typeof(IGaia), birthMark));
            ValidCelestials = TemplateCache.GetAll<ICelestial>(true);
            ValidInanimateTemplates = TemplateCache.GetAll<IInanimateTemplate>(true);
        }

        public IEnumerable<ICelestial> ValidCelestials { get; set; }
        public IEnumerable<IInanimateTemplate> ValidInanimateTemplates { get; set; }

        [UIHint("Gaia")]
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

    public class ViewZoneViewModel : LiveEntityViewModel
    {
        public ViewZoneViewModel()
        {
        }

        public ViewZoneViewModel(string birthMark)
        {
            DataObject = LiveCache.Get<IZone>(new LiveCacheKey(typeof(IZone), birthMark));
        }

        [UIHint("Zone")]
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

    public class ViewInanimateViewModel : LiveEntityViewModel
    {
        public ViewInanimateViewModel()
        {
            ValidMaterials = TemplateCache.GetAll<IMaterial>(true);
            ValidModels = TemplateCache.GetAll<IDimensionalModelData>();
        }

        public ViewInanimateViewModel(string birthMark)
        {
            DataObject = LiveCache.Get<IInanimate>(new LiveCacheKey(typeof(IInanimate), birthMark));
            ValidMaterials = TemplateCache.GetAll<IMaterial>(true);
            ValidModels = TemplateCache.GetAll<IDimensionalModelData>();
        }

        public IEnumerable<IMaterial> ValidMaterials { get; set; }
        public IEnumerable<IDimensionalModelData> ValidModels { get; set; }

        [UIHint("Inanimate")]
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

    public class ViewIntelligenceViewModel : LiveEntityViewModel
    {
        public ViewIntelligenceViewModel()
        {
        }

        public ViewIntelligenceViewModel(string birthMark)
        {
            DataObject = LiveCache.Get<INonPlayerCharacter>(new LiveCacheKey(typeof(INonPlayerCharacter), birthMark));
        }

        [UIHint("NonPlayerCharacter")]
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

    public class ViewRoomViewModel : LiveEntityViewModel
    {
        public ViewRoomViewModel()
        {
            ValidMaterials = TemplateCache.GetAll<IMaterial>(true);
            ValidModels = TemplateCache.GetAll<IDimensionalModelData>();
        }

        public ViewRoomViewModel(string birthMark)
        {
            DataObject = LiveCache.Get<IRoom>(new LiveCacheKey(typeof(IRoom), birthMark));
            ValidMaterials = TemplateCache.GetAll<IMaterial>(true);
            ValidModels = TemplateCache.GetAll<IDimensionalModelData>();
        }

        public IEnumerable<IMaterial> ValidMaterials { get; set; }
        public IEnumerable<IDimensionalModelData> ValidModels { get; set; }

        [UIHint("Room")]
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

    public class ViewLocaleViewModel : LiveEntityViewModel
    {
        public ViewLocaleViewModel()
        {
        }

        public ViewLocaleViewModel(string birthMark)
        {
            DataObject = LiveCache.Get<ILocale>(new LiveCacheKey(typeof(ILocale), birthMark));
        }

        [UIHint("Locale")]
        public ILocale DataObject { get; set; }
    }
}