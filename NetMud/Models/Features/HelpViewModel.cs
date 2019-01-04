using NetMud.Authentication;
using NetMud.DataStructure.Administrative;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.Features
{
    public class HelpViewModel : IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public IEnumerable<IHelp> Items { get; set; }

        [Display(Name = "Search Term", Description = "Filter by the subject or body of the help files.")]
        [DataType(DataType.Text)]
        public string SearchTerm { get; set; }

        [Display(Name = "In-Game", Description = "Include help content from in-game entity types and commands.")]
        [UIHint("Boolean")]
        public bool IncludeInGame { get; set; }

        public HelpViewModel(IEnumerable<IHelp> items)
        {
            Items = items;
        }
    }
}