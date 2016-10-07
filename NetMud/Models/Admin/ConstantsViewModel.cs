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
        
        [Display(Name = "Name")]
        public string NewName { get; set; }

        
        [Display(Name = "Criterion Type")]
        public short[] NewCriterionTypes { get; set; }

        
        [Display(Name = "Criterion Value")]
        public string[] NewCriterionValues { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Value")]
        public string[] NewConstantValues { get; set; }

        public IConstants DataObject { get; set; }
    }
}