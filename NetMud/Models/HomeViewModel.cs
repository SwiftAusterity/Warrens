using NetMud.Authentication;
using NetMud.DataStructure.Administrative;
using System.Collections.Generic;

namespace NetMud.Models
{
    public class HomeViewModel : IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public IJournalEntry LatestPatchNotes { get; set; }
        public IEnumerable<IJournalEntry> LatestNews { get; set; }

        public HomeViewModel()
        {
        }
    }
}