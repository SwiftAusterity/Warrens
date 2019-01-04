using NetMud.Authentication;
using NetMud.DataStructure.Administrative;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace NetMud.Models.Admin
{
    public class ManageHelpDataViewModel : PagedDataModel<IHelp>, IBaseViewModel
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

    public class AddEditHelpDataViewModel : IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditHelpDataViewModel()
        {
        }

        [UIHint("Help")]
        public IHelp DataObject { get; set; }
    }
}