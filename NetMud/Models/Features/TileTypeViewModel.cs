using NetMud.Authentication;
using NetMud.DataStructure.Tile;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.Features
{
    public class TileTypeViewModel : IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public IEnumerable<ITileTemplate> Items { get; set; }

        [Display(Name = "Search Term", Description = "Filter by the name of the item.")]
        [DataType(DataType.Text)]
        public string SearchTerm { get; set; }

        public TileTypeViewModel(IEnumerable<ITileTemplate> items)
        {
            Items = items;
        }
    }
}