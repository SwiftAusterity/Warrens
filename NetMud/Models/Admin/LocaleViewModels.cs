using NetMud.Authentication;
using NetMud.DataStructure.Locale;
using System;
using System.Collections.Generic;


namespace NetMud.Models.Admin
{
    public class ManageLocaleTemplateViewModel : PagedDataModel<ILocaleTemplate>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageLocaleTemplateViewModel(IEnumerable<ILocaleTemplate> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<ILocaleTemplate, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }
    }

    public class AddEditLocaleTemplateViewModel : IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }
        public long ZoneId { get; set; }

        public AddEditLocaleTemplateViewModel()
        {
        }

        public ILocaleTemplate DataObject { get; set; }
    }
}