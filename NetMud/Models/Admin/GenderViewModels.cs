using NetMud.Authentication;
using NetMud.Data.Architectural.ActorBase;
using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataStructure.Architectural.ActorBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.Admin
{
    public class ManageGenderDataViewModel : PagedDataModel<IGender>
    {
        public ManageGenderDataViewModel(IEnumerable<IGender> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IGender, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower()) || item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }

        internal override Func<IGender, object> OrderPrimary
        {
            get
            {
                return item => item.Name;
            }
        }


        internal override Func<IGender, object> OrderSecondary
        {
            get
            {
                return null;
            }
        }
    }

    public class AddEditGenderViewModel : AddEditTemplateModel<IGender>
    {
        [Display(Name = "Apply Existing Template", Description = "Apply an existing object's data to this new data.")]
        [UIHint("NonPlayerCharacterTemplateList")]
        [GenderDataBinder]
        public override IGender Template { get; set; }

        public AddEditGenderViewModel() : base(-1)
        {
            DataObject = new Gender();
        }

        public AddEditGenderViewModel(long templateId) : base(templateId)
        {
            DataObject = new Gender();

            //apply template
            if (DataTemplate != null)
            {
                DataObject.Adult = DataTemplate.Adult;
                DataObject.Base = DataTemplate.Base;
                DataObject.Child = DataTemplate.Child;
                DataObject.Collective = DataTemplate.Collective;
                DataObject.Feminine = DataTemplate.Feminine;
                DataObject.HelpText = DataTemplate.HelpText;
                DataObject.Possessive = DataTemplate.Possessive;
            }
        }

        public AddEditGenderViewModel(string archivePath, IGender item) : base(archivePath, item)
        {
            DataObject = item;
        }

        public IGender DataObject { get; set; }
    }
}