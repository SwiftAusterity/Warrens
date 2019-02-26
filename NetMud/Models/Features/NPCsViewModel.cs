using NetMud.Authentication;
using NetMud.DataStructure.NPC;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.Features
{
    public class NPCsViewModel : IBaseViewModel
    {
        public ApplicationUser AuthedUser { get; set; }

        public IEnumerable<INonPlayerCharacterTemplate> Items { get; set; }

        [Display(Name = "Search Term", Description = "Filter by the name of the item.")]
        [DataType(DataType.Text)]
        public string SearchTerm { get; set; }

        public NPCsViewModel(IEnumerable<INonPlayerCharacterTemplate> items)
        {
            Items = items;
        }
    }
}