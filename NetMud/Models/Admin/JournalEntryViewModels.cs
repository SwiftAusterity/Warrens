using NetMud.Authentication;
using NetMud.DataStructure.Base.Supporting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.Admin
{
    public class ManageJournalEntriesViewModel : PagedDataModel<IJournalEntry>, BaseViewModel
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

    public class AddEditJournalEntryViewModel : TwoDimensionalEntityEditViewModel
    {
        public AddEditJournalEntryViewModel()
        {
        }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 5)]
        [Display(Name = "Subject Line", Description = "The subject line of the entry.")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [StringLength(2000, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 20)]
        [Display(Name = "Body", Description = "The body content of the entry.")]
        [DataType("Markdown")]
        public string Body { get; set; }

        [Display(Name = "Is Public?", Description = "Can this be seen by people who are not logged in. Overrides Minimum Read Level if true.")]
        bool Public { get; set; }

        [Display(Name = "Force Expiry", Description = "If set to true will be considered expired no matter what the date is.")]
        bool Expired { get; set; }

        [Display(Name = "Minimum Read Level", Description = "Sets the minimum rank someone's account must be to see this.")]
        short MinimumReadLevel { get; set; }

        [Display(Name = "Publish On", Description = "The date this will be considered active and available to see.")]
        [DataType(DataType.Date)]
        DateTime PublishDate { get; set; }

        [Display(Name = "Expires On", Description = "The date this will be considered expired.")]
        [DataType(DataType.Date)]
        DateTime ExpireDate { get; set; }

        [Display(Name = "Tags", Description = "Filtering tags such as Blog, Patch Notes, Update, etc.")]
        [DataType(DataType.Text)]
        string Tags { get; set; }

        public IJournalEntry DataObject { get; set; }
    }
}