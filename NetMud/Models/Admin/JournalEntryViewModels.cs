using NetMud.Authentication;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace NetMud.Models.Admin
{
    public class ManageJournalEntriesViewModel : PagedCacheModel<IJournalEntry>
    {
        public ManageJournalEntriesViewModel()
            : base(CacheType.LookupData)
        {
        }

        internal override Func<IJournalEntry, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower()) || item.Body.ToLower().Contains(SearchTerms.ToLower());
            }
        }

        internal override Func<IJournalEntry, object> OrderPrimary
        {
            get
            {
                return item => item.Expired ? 0 : 1;
            }
        }


        internal override Func<IJournalEntry, object> OrderSecondary
        {
            get
            {
                return item => item.PublishDate;
            }
        }
    }

    public class AddEditJournalEntryViewModel : IBaseViewModel
    {
        public ApplicationUser AuthedUser { get; set; }

        public AddEditJournalEntryViewModel()
        {
        }

        [AllowHtml]
        [UIHint("JournalEntry")]
        public IJournalEntry DataObject { get; set; }
    }
}