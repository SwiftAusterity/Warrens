using NetMud.Authentication;
using NetMud.DataStructure.Linguistic;
using System;
using System.Collections.Generic;


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

        internal override Func<ILanguage, object> OrderPrimary
        {
            get
            {
                return item => item.UIOnly ? 0 : 1;
            }
        }


        internal override Func<ILanguage, object> OrderSecondary
        {
            get
            {
                return item => item.Name;
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