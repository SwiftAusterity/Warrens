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

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Name", Description = "The descriptive name for this language. Used to find and refer to it in code.")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [Display(Name = "Language Code", Description = "The language code Google Translate uses to identify this language.")]
        [DataType(DataType.Text)]
        public string GoogleLanguageCode { get; set; }

        [Display(Name = "UI Only", Description = "Only for use in translating the input/output, not an 'in game' language.")]
        public bool UIOnly { get; set; }

        public ILanguage DataObject { get; set; }
    }
}