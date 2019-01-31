using NetMud.Authentication;
using NetMud.DataStructure.Gaia;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.Features
{
    public class CelestialsViewModel : IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public IEnumerable<ICelestial> Items { get; set; }

        [Display(Name = "Search Term", Description = "Filter by the name of the item.")]
        [DataType(DataType.Text)]
        public string SearchTerm { get; set; }

        public CelestialsViewModel(IEnumerable<ICelestial> items)
        {
            Items = items;
        }
    }
}