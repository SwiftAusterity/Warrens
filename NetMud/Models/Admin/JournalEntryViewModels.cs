using NetMud.Authentication;
using NetMud.DataStructure.Administrative;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.Admin
{
    public class ManageJournalEntriesViewModel : PagedDataModel<IJournalEntry>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageJournalEntriesViewModel(IEnumerable<IJournalEntry> items)
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

    public class AddEditJournalEntryViewModel : IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditJournalEntryViewModel()
        {
        }

        [UIHint("JournalEntry")]
        public IJournalEntry DataObject { get; set; }
    }
}