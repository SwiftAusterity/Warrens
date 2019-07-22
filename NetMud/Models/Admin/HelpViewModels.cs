using NetMud.Data.Administrative;
using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataStructure.Administrative;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace NetMud.Models.Admin
{
    public class ManageHelpDataViewModel : PagedDataModel<IHelp>
    {
        public ManageHelpDataViewModel(IEnumerable<IHelp> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IHelp, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower()) || item.HelpText.ToLower().Contains(SearchTerms.ToLower());
            }
        }

        internal override Func<IHelp, object> OrderPrimary
        {
            get
            {
                return item => item.Name;
            }
        }


        internal override Func<IHelp, object> OrderSecondary
        {
            get
            {
                return null;
            }
        }
    }

    public class AddEditHelpDataViewModel : AddEditTemplateModel<IHelp>
    {
        [Display(Name = "Apply Existing Template", Description = "Apply an existing object's data to this new data.")]
        [UIHint("NonPlayerCharacterTemplateList")]
        [HelpDataBinder]
        public override IHelp Template { get; set; }

        public AddEditHelpDataViewModel() : base(-1)
        {
            DataObject = new Help();
        }

        public AddEditHelpDataViewModel(long templateId) : base(templateId)
        {
            DataObject = new Help();

            //apply template
            if (DataTemplate != null)
            {
                DataObject.HelpText = DataTemplate.HelpText;
            }
        }

        public AddEditHelpDataViewModel(string archivePath, IHelp item) : base(archivePath, item)
        {
            DataObject = (Help)item;
        }

        [AllowHtml]
        [UIHint("Help")]
        public Help DataObject { get; set; }
    }
}