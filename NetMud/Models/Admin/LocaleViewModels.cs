using NetMud.Authentication;
using NetMud.DataStructure.Locale;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


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

        [StringLength(100, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Name", Description = "The descriptive name for this Locale. Shows above the output window of the client.")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        public ILocaleTemplate DataObject { get; set; }
    }
}