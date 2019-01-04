using NetMud.Authentication;
using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataStructure.Inanimate;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace NetMud.Models.Admin
{
    public class ManageInanimateDataViewModel : PagedDataModel<IInanimateTemplate>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageInanimateDataViewModel(IEnumerable<IInanimateTemplate> items)
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
    }

    public class AddEditInanimateDataViewModel : AddContentModel<IInanimateTemplate>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        [Display(Name = "Apply Existing Template", Description = "Apply an existing object's data to this new data.")]
        [UIHint("InanimateTemplateList")]
        [InanimateTemplateDataBinder]
        public override IInanimateTemplate Template { get; set; }

        public AddEditInanimateDataViewModel() : base(-1)
        {
        }

        public AddEditInanimateDataViewModel(long templateId) : base(templateId)
        {
            //apply template
            if (DataTemplate == null)
            {
                //set defaults
            }
            else
            {

            }
        }

        public IEnumerable<IInanimateTemplate> ValidComponents { get; set; }

        [UIHint("InanimateTemplate")]
        public IInanimateTemplate DataObject { get; set; }
    }
}