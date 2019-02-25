using NetMud.Authentication;
using NetMud.Data.Architectural.PropertyBinding;
using NetMud.Data.NaturalResource;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.NaturalResource;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;


namespace NetMud.Models.Admin
{
    public class ManageFaunaViewModel : PagedDataModel<IFauna>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageFaunaViewModel(IEnumerable<IFauna> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IFauna, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }

        internal override Func<IFauna, object> OrderPrimary
        {
            get
            {
                return item => item.Name;
            }
        }


        internal override Func<IFauna, object> OrderSecondary
        {
            get
            {
                return null;
            }
        }
    }

    public class AddEditFaunaViewModel : AddEditTemplateModel<IFauna>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        [Display(Name = "Apply Existing Template", Description = "Apply an existing object's data to this new data.")]
        [UIHint("FaunaList")]
        [FaunaDataBinder]
        public override IFauna Template { get; set; }

        public AddEditFaunaViewModel() : base(-1)
        {
            ValidInanimateTemplates = TemplateCache.GetAll<IInanimateTemplate>();
            ValidMaterials = TemplateCache.GetAll<IMaterial>();
            ValidRaces = TemplateCache.GetAll<IRace>();
            DataObject = new Fauna();
        }

        public AddEditFaunaViewModel(long templateId) : base(templateId)
        {
            ValidInanimateTemplates = TemplateCache.GetAll<IInanimateTemplate>();
            ValidMaterials = TemplateCache.GetAll<IMaterial>();
            ValidRaces = TemplateCache.GetAll<IRace>();
            DataObject = new Fauna();

            //apply template
            if (DataTemplate != null)
            {
                DataObject.AmountMultiplier = DataTemplate.AmountMultiplier;
                DataObject.CanSpawnInSystemAreas = DataTemplate.CanSpawnInSystemAreas;
                DataObject.ElevationRange = DataTemplate.ElevationRange;
                DataObject.FemaleRatio = DataTemplate.FemaleRatio;
                DataObject.HumidityRange = DataTemplate.HumidityRange;
                DataObject.OccursIn = DataTemplate.OccursIn;
                DataObject.PopulationHardCap = DataTemplate.PopulationHardCap;
                DataObject.PuissanceVariance = DataTemplate.PuissanceVariance;
                DataObject.Rarity = DataTemplate.Rarity;
                DataObject.Race = DataTemplate.Race;
                DataObject.TemperatureRange = DataTemplate.TemperatureRange;
            }
        }

        public IEnumerable<IRace> ValidRaces { get; set; }
        public IEnumerable<IInanimateTemplate> ValidInanimateTemplates { get; set; }
        public IEnumerable<IMaterial> ValidMaterials { get; set; }
        public IFauna DataObject { get; set; }
    }
}