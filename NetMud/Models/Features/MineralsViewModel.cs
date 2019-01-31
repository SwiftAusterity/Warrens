using NetMud.Authentication;
using NetMud.DataStructure.NaturalResource;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.Features
{
    public class MineralsViewModel : IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public IEnumerable<IMineral> Items { get; set; }

        [Display(Name = "Search Term", Description = "Filter by the name of the item.")]
        [DataType(DataType.Text)]
        public string SearchTerm { get; set; }

        public MineralsViewModel(IEnumerable<IMineral> items)
        {
            Items = items;
        }
    }
}