using NetMud.Authentication;
using NetMud.DataStructure.Inanimate;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.Features
{
    public class ItemsViewModel : IBaseViewModel
    {
        public ApplicationUser AuthedUser { get; set; }

        public IEnumerable<IInanimateTemplate> Items { get; set; }

        [Display(Name = "Search Term", Description = "Filter by the name of the item.")]
        [DataType(DataType.Text)]
        public string SearchTerm { get; set; }

        public ItemsViewModel(IEnumerable<IInanimateTemplate> items)
        {
            Items = items;
        }
    }
}