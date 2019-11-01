using NetMud.Authentication;
using NetMud.DataStructure.Combat;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.Features
{
    public class FightingArtsViewModel : IBaseViewModel
    {
        public ApplicationUser AuthedUser { get; set; }

        public IEnumerable<IFightingArt> Items { get; set; }

        [Display(Name = "Search Term", Description = "Filter by the name of the fighting art.")]
        [DataType(DataType.Text)]
        public string SearchTerm { get; set; }

        public FightingArtsViewModel(IEnumerable<IFightingArt> items)
        {
            Items = items;
        }
    }
}