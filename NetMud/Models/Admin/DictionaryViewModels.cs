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

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Name", Description = "The Word/Phrase at hand. Used to find and refer to it in code.")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [Display(Name = "Type", Description = "The type of word this is.")]
        public short Type { get; set; }

        [Display(Name = "Tense", Description = "Chronological tense (past, present, future)")]
        public short Tense { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Severity", Description = "Strength rating of word in relation to synonyms.")]
        [DataType(DataType.Text)]
        public int Severity { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Elegance", Description = "Crudeness rating of word in relation to synonyms.")]
        [DataType(DataType.Text)]
        public int Elegance { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Quality", Description = "Finesse synonym rating; quality of execution of form or function.")]
        [DataType(DataType.Text)]
        public int Quality { get; set; }

        [Display(Name = "Synonyms", Description = "The synonyms (similar) of this word/phrase.")]
        [DataType(DataType.Text)]
        public string Synonyms { get; set; }

        [Display(Name = "Antonyms", Description = "The antonyms (opposite) of this word/phrase.")]
        [DataType(DataType.Text)]
        public string Antonyms { get; set; }

        public IDictata DataObject { get; set; }
    }
}