using NetMud.Authentication;
using NetMud.DataStructure.Base.System;
using System;
using System.Collections.Generic;

namespace NetMud.Models.Admin
{
    public class ManageContentApprovalsViewModel : PagedDataModel<IKeyedData>, BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageContentApprovalsViewModel(IEnumerable<IKeyedData> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IKeyedData, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }
    }

}