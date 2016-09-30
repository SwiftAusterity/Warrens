using NetMud.Authentication;
using NetMud.DataStructure.Base.System;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace NetMud.Models.Admin
{
    public class ManageConstantsDataViewModel : PagedDataModel<IConstants>, BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageConstantsDataViewModel(IEnumerable<IConstants> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IConstants, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }
    }

    public class AddEditConstantsViewModel : BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditConstantsViewModel()
        {
        }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [DataType(DataType.Text)]
        [Display(Name = "Name")]
        public string NewName { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Criterion Type")]
        public short[] NewCriterionTypes { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 1)]
        [DataType(DataType.Text)]
        [Display(Name = "Criterion Value")]
        public string[] NewCriterionValues { get; set; }

        [StringLength(2000, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 1)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Value")]
        public string NewConstantValue { get; set; }

        public IConstants DataObject { get; set; }
    }
}