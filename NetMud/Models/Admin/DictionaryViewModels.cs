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

        internal override Func<IDictata, object> OrderPrimary
        {
            get
            {
                return item => item.Language.Name;
            }
        }


        internal override Func<IDictata, object> OrderSecondary
        {
            get
            {
                return item => item.Name;
            }
        }
    }

    public class AddEditDictionaryViewModel : IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditDictionaryViewModel()
        {
        }

        [Display(Name = "Word", Description = "The new word's name/spelling.")]
        [DataType(DataType.Text)]
        public string Word { get; set; }

        [Display(Name = "Is Synonym", Description = "Is this a synonym (true) or an antonym (false) of the current word.")]
        public bool Synonym { get; set; }

        [Display(Name = "Elegance", Description = "The quality Delta against the current word.")]
        [DataType(DataType.Text)]
        public int Elegance { get; set; }

        [Display(Name = "Severity", Description = "The quality Delta against the current word.")]
        [DataType(DataType.Text)]
        public int Severity { get; set; }

        [Display(Name = "Quality", Description = "The quality Delta against the current word.")]
        [DataType(DataType.Text)]
        public int Quality { get; set; }

        public IEnumerable<ILanguage> ValidLanguages { get; set; }
        public IEnumerable<IDictata> ValidWords { get; set; }
        public IDictata DataObject { get; set; }
    }
}