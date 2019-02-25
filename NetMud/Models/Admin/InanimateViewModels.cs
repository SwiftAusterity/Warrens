using NetMud.Authentication;
using NetMud.Data.Architectural.PropertyBinding;
using NetMud.Data.Inanimate;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Inanimate;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;


namespace NetMud.Models.Admin
{
    public class ManageInanimateTemplateViewModel : PagedDataModel<IInanimateTemplate>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageInanimateTemplateViewModel(IEnumerable<IInanimateTemplate> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IInanimateTemplate, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }

        internal override Func<IInanimateTemplate, object> OrderPrimary
        {
            get
            {
                return item => item.Name;
            }
        }


        internal override Func<IInanimateTemplate, object> OrderSecondary
        {
            get
            {
                return null;
            }
        }
    }

    public class AddEditInanimateTemplateViewModel : AddContentModel<IInanimateTemplate>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        [Display(Name = "Apply Existing Template", Description = "Apply an existing object's data to this new data.")]
        [UIHint("NonPlayerCharacterTemplateList")]
        [InanimateTemplateDataBinder]
        public override IInanimateTemplate Template { get; set; }

        public AddEditInanimateTemplateViewModel() : base(-1)
        {
            ValidMaterials = TemplateCache.GetAll<IMaterial>();
            ValidModels = TemplateCache.GetAll<IDimensionalModelData>().Where(model => model.ModelType == DimensionalModelType.Flat);
            ValidInanimateTemplates = TemplateCache.GetAll<IInanimateTemplate>();
            DataObject = new InanimateTemplate();
        }

        public AddEditInanimateTemplateViewModel(long templateId) : base(templateId)
        {
            ValidMaterials = TemplateCache.GetAll<IMaterial>();
            ValidModels = TemplateCache.GetAll<IDimensionalModelData>().Where(model => model.ModelType == DimensionalModelType.Flat);
            ValidInanimateTemplates = TemplateCache.GetAll<IInanimateTemplate>();
            DataObject = new InanimateTemplate();

            //apply template
            if (DataTemplate != null)
            {
                DataObject.AccumulationCap = DataTemplate.AccumulationCap;
                DataObject.Components = DataTemplate.Components;
                DataObject.MobileContainers = DataTemplate.MobileContainers;
                DataObject.InanimateContainers = DataTemplate.InanimateContainers;
                DataObject.Qualities = DataTemplate.Qualities;
                DataObject.Model = DataTemplate.Model;
                DataObject.Produces = DataTemplate.Produces;
                DataObject.SkillRequirements = DataTemplate.SkillRequirements;
            }
        }

        public IEnumerable<IInanimateTemplate> ValidInanimateTemplates { get; set; }

        [UIHint("InanimateTemplate")]
        public IInanimateTemplate DataObject { get; set; }
        public IEnumerable<IDimensionalModelData> ValidModels { get; set; }
        public IEnumerable<IMaterial> ValidMaterials { get; set; }
    }
}