using NetMud.Authentication;
using NetMud.DataStructure.Architectural.ActorBase;
using System;
using System.Collections.Generic;

namespace NetMud.Models.Admin
{
    public class ManageGenderDataViewModel : PagedDataModel<IGender>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageGenderDataViewModel(IEnumerable<IGender> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IGender, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower()) || item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }
    }

    public class AddEditGenderViewModel : IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditGenderViewModel()
        {
        }

        public IGender DataObject { get; set; }
    }
}