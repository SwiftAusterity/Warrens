using NetMud.DataAccess.Cache;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.Admin
{
    public class ManageNPCDataViewModel : PagedDataModel<INonPlayerCharacterTemplate>
    {
        public ManageNPCDataViewModel(IEnumerable<INonPlayerCharacterTemplate> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<INonPlayerCharacterTemplate, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower()) || item.SurName.ToLower().Contains(SearchTerms.ToLower());
            }
        }

        internal override Func<INonPlayerCharacterTemplate, object> OrderPrimary
        {
            get
            {
                return item => item.Name;
            }
        }


        internal override Func<INonPlayerCharacterTemplate, object> OrderSecondary
        {
            get
            {
                return null;
            }
        }
    }

    public class AddEditNPCDataViewModel : AddEditTemplateModel<INonPlayerCharacterTemplate>
    {
        [Display(Name = "Apply Existing Template", Description = "Apply an existing object's data to this new data.")]
        [UIHint("NonPlayerCharacterTemplateList")]
        [NonPlayerCharacterTemplateDataBinder]
        public override INonPlayerCharacterTemplate Template { get; set; }

        public AddEditNPCDataViewModel() : base(-1)
        {
            ValidItems = TemplateCache.GetAll<IInanimateTemplate>();
            ValidRaces = TemplateCache.GetAll<IRace>();
            ValidGenders = TemplateCache.GetAll<IGender>();
            DataObject = new NonPlayerCharacterTemplate();
        }

        public AddEditNPCDataViewModel(long templateId) : base(templateId)
        {
            ValidItems = TemplateCache.GetAll<IInanimateTemplate>();
            ValidRaces = TemplateCache.GetAll<IRace>();
            ValidGenders = TemplateCache.GetAll<IGender>();
            DataObject = new NonPlayerCharacterTemplate();

            //apply template
            if (DataTemplate != null)
            {
                DataObject.Gender = DataTemplate.Gender;
                DataObject.InventoryRestock = DataTemplate.InventoryRestock;
                DataObject.Personality = DataTemplate.Personality;
                DataObject.Qualities = DataTemplate.Qualities;
                DataObject.Race = DataTemplate.Race;
                DataObject.TeachableProficencies = DataTemplate.TeachableProficencies;
                DataObject.TotalHealth = DataTemplate.TotalHealth;
                DataObject.TotalStamina = DataTemplate.TotalStamina;
                DataObject.WillPurchase = DataTemplate.WillPurchase;
                DataObject.WillSell = DataTemplate.WillSell;
            }
        }

        public AddEditNPCDataViewModel(string archivePath, INonPlayerCharacterTemplate item) : base(archivePath, item)
        {
            ValidItems = TemplateCache.GetAll<IInanimateTemplate>();
            ValidRaces = TemplateCache.GetAll<IRace>();
            ValidGenders = TemplateCache.GetAll<IGender>();
            DataObject = item;
        }

        public IEnumerable<IGender> ValidGenders { get; set; }
        public IEnumerable<IRace> ValidRaces { get; set; }
        public IEnumerable<IInanimateTemplate> ValidItems { get; set; }

        [UIHint("NonPlayerCharacterTemplate")]
        public INonPlayerCharacterTemplate DataObject { get; set; }
    }
}