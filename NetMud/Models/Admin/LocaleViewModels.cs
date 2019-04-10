using NetMud.Authentication;
using System;
using System.Collections.Generic;


namespace NetMud.Models.Admin
{
    public class ManageLocaleTemplateViewModel : PagedDataModel<ILocaleTemplate>
    {
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

        internal override Func<ILocaleTemplate, object> OrderPrimary
        {
            get
            {
                return item => item.Name;
            }
        }


        internal override Func<ILocaleTemplate, object> OrderSecondary
        {
            get
            {
                return null;
            }
        }
    }

    public class AddEditLocaleTemplateViewModel : IBaseViewModel
    {
        public ApplicationUser AuthedUser { get; set; }
        public long ZoneId { get; set; }

        public AddEditLocaleTemplateViewModel()
        {
        }

        public ILocaleTemplate DataObject { get; set; }
    }
}