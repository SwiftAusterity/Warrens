using NetMud.Authentication;
using NetMud.Data.LookupData;
using NetMud.DataStructure.Base.System;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace NetMud.Models.Admin
{
    public class ManageHelpDataViewModel : PagedDataModel<IHelp>, BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

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
    }

    public class AddEditHelpDataViewModel : BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditHelpDataViewModel()
        {
        }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        
        [Display(Name = "Name")]
        public string NewName { get; set; }

        [StringLength(2000, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 20)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Help Text")]
        public string NewHelpText { get; set; }

        public IHelp DataObject { get; set; }
    }
}