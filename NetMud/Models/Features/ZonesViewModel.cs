using NetMud.Authentication;
using NetMud.DataStructure.Zone;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.Features
{
    public class ZonesViewModel : IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public IEnumerable<IZoneTemplate> Items { get; set; }

        [Display(Name = "Search Term", Description = "Filter by the name of the item.")]
        [DataType(DataType.Text)]
        public string SearchTerm { get; set; }

        public ZonesViewModel(IEnumerable<IZoneTemplate> items)
        {
            Items = items;
        }
    }
}