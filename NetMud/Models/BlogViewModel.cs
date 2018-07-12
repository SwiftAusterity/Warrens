using NetMud.Authentication;
using NetMud.DataStructure.Base.Supporting;
using System;
using System.Collections.Generic;

namespace NetMud.Models
{
    public class BlogViewModel : PagedDataModel<IJournalEntry>, BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public BlogViewModel(IEnumerable<IJournalEntry> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IJournalEntry, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower()) || item.Body.ToLower().Contains(SearchTerms.ToLower());
            }
        }

    }
}