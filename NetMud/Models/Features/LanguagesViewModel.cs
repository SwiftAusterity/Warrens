using NetMud.Authentication;
using NetMud.DataStructure.Linguistic;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.Features
{
    public class LanguagesViewModel : IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public IEnumerable<ILanguage> Items { get; set; }

        [Display(Name = "Search Term", Description = "Filter by the name of the item.")]
        [DataType(DataType.Text)]
        public string SearchTerm { get; set; }

        public LanguagesViewModel(IEnumerable<ILanguage> items)
        {
            Items = items;
        }
    }
}