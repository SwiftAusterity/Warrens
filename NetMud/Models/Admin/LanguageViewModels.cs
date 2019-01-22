using NetMud.Authentication;
using NetMud.DataStructure.Linguistic;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace NetMud.Models.Admin
{
    public class ManageLanguageDataViewModel : PagedDataModel<ILanguage>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageLanguageDataViewModel(IEnumerable<ILanguage> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<ILanguage, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }
    }

    public class AddEditLanguageViewModel : IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditLanguageViewModel()
        {
        }

        public ILanguage DataObject { get; set; }
    }
}