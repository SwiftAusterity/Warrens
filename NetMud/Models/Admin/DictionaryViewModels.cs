using NetMud.Authentication;
using NetMud.DataStructure.Linguistic;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.Admin
{
    public class ManageDictionaryViewModel : PagedDataModel<IDictata>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageDictionaryViewModel(IEnumerable<IDictata> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IDictata, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }
    }

    public class AddEditDictionaryViewModel : IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditDictionaryViewModel()
        {
        }

        public IEnumerable<ILanguage> ValidLanguages { get; set; }
        public IEnumerable<IDictata> ValidWords { get; set; }
        public IDictata DataObject { get; set; }
    }
}