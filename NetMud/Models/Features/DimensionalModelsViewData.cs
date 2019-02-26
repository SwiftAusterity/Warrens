using NetMud.Authentication;
using NetMud.DataStructure.Architectural.EntityBase;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.Features
{
    public class DimensionalModelsViewData : IBaseViewModel
    {
        public ApplicationUser AuthedUser { get; set; }

        public IEnumerable<IDimensionalModel> Items { get; set; }

        [Display(Name = "Search Term", Description = "Filter by the name of the item.")]
        [DataType(DataType.Text)]
        public string SearchTerm { get; set; }

        public DimensionalModelsViewData(IEnumerable<IDimensionalModel> items)
        {
            Items = items;
        }
    }
}