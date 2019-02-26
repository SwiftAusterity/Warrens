using NetMud.Authentication;
using NetMud.DataStructure.Administrative;
using System;
using System.Collections.Generic;

namespace NetMud.Models
{
    public class BlogViewModel : IBaseViewModel
    {
        public ApplicationUser AuthedUser { get; set; }
        public IEnumerable<IJournalEntry> Items { get; set; }
        public IEnumerable<Tuple<string, int>> MonthYearPairs { get; set; }
        public string[] IncludeTags { get; set; }
        public string[] AllTags { get; set; }

        public BlogViewModel(IEnumerable<IJournalEntry> items)
        {
            Items = items;
            IncludeTags = new string[0];
            AllTags = new string[0];
        }
    }
}