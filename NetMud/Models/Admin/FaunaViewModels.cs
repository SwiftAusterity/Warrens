using NetMud.Authentication;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace NetMud.Models.Admin
{
    public class ManageFaunaViewModel : PagedDataModel<IFauna>, BaseViewModel
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
    }

    public class AddEditFaunaViewModel : BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditFaunaViewModel()
        {
        }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Name")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [StringLength(2000, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 20)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Help Text")]
        public string HelpText { get; set; }

        public IFauna DataObject { get; set; }
    }
}