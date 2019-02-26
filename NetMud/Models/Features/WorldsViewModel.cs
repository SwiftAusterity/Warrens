using NetMud.Authentication;
using NetMud.DataStructure.Gaia;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.Features
{
    public class WorldsViewModel : IBaseViewModel
    {
        public ApplicationUser AuthedUser { get; set; }

        public IEnumerable<IGaiaTemplate> Items { get; set; }

        [Display(Name = "Search Term", Description = "Filter by the name of the item.")]
        [DataType(DataType.Text)]
        public string SearchTerm { get; set; }

        public WorldsViewModel(IEnumerable<IGaiaTemplate> items)
        {
            Items = items;
        }
    }
}