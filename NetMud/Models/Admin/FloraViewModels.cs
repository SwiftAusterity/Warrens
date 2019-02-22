using NetMud.Authentication;
using NetMud.Data.Architectural.PropertyBinding;
using NetMud.Data.NaturalResource;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.NaturalResource;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;


namespace NetMud.Models.Admin
{
    public class ManageFloraViewModel : PagedDataModel<IFlora>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageFloraViewModel(IEnumerable<IFlora> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IFlora, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }

        internal override Func<IFlora, object> OrderPrimary
        {
            get
            {
                return item => item.Name;
            }
        }


        internal override Func<IFlora, object> OrderSecondary
        {
            get
            {
                return null;
            }
        }
    }

    public class AddEditFloraViewModel : AddContentModel<IFlora>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        [Display(Name = "Apply Existing Template", Description = "Apply an existing object's data to this new data.")]
        [UIHint("FloraList")]
        [FloraDataBinder]
        public override IFlora Template { get; set; }

        public AddEditFloraViewModel() : base(-1)
        {
            ValidInanimateTemplates = TemplateCache.GetAll<IInanimateTemplate>();
            ValidMaterials = TemplateCache.GetAll<IMaterial>();
            DataObject = new Flora();
        }

        public AddEditFloraViewModel(long templateId) : base(templateId)
        {
            ValidInanimateTemplates = TemplateCache.GetAll<IInanimateTemplate>();
            ValidMaterials = TemplateCache.GetAll<IMaterial>();
            DataObject = new Flora();

            //apply template
            if (DataTemplate != null)
            {
                DataObject.AmountMultiplier = DataTemplate.AmountMultiplier;
                DataObject.CanSpawnInSystemAreas = DataTemplate.CanSpawnInSystemAreas;
                DataObject.ElevationRange = DataTemplate.ElevationRange;
                DataObject.HumidityRange = DataTemplate.HumidityRange;
                DataObject.OccursIn = DataTemplate.OccursIn;
                DataObject.PuissanceVariance = DataTemplate.PuissanceVariance;
                DataObject.Rarity = DataTemplate.Rarity;
                DataObject.TemperatureRange = DataTemplate.TemperatureRange;

                DataObject.Coniferous = DataTemplate.Coniferous;
                DataObject.Flower = DataTemplate.Flower;
                DataObject.Fruit = DataTemplate.Fruit;
                DataObject.Leaf = DataTemplate.Leaf;
                DataObject.Seed = DataTemplate.Seed;
                DataObject.SunlightPreference = DataTemplate.SunlightPreference;
                DataObject.Wood = DataTemplate.Wood;
            }
        }

        public IEnumerable<IInanimateTemplate> ValidInanimateTemplates { get; set; }
        public IEnumerable<IMaterial> ValidMaterials { get; set; }
        public IFlora DataObject { get; set; }
    }
}