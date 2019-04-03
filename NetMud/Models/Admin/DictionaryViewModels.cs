using NetMud.Authentication;
using NetMud.Data.Architectural.PropertyBinding;
using NetMud.Data.Linguistic;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Linguistic;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace NetMud.Models.Admin
{
    public class ManageDictionaryViewModel : PagedDataModel<ILexeme>
    {
        public ManageDictionaryViewModel(IEnumerable<ILexeme> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<ILexeme, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }

        internal override Func<ILexeme, object> OrderPrimary
        {
            get
            {
                return item => item.Language.Name;
            }
        }


        internal override Func<ILexeme, object> OrderSecondary
        {
            get
            {
                return item => item.Name;
            }
        }
    }

    public class AddEditDictionaryViewModel : PagedDataModel<IDictata>
    {
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
                return item => item.Name;
            }
        }


        internal override Func<IDictata, object> OrderSecondary
        {
            get
            {
                return item => item.FormGroup;
            }
        }

        public AddEditDictionaryViewModel(IEnumerable<IDictata> items)
        : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
            ValidWords = ConfigDataCache.GetAll<ILexeme>();
            ValidPhrases = ConfigDataCache.GetAll<IDictataPhrase>();
            ValidLanguages = ConfigDataCache.GetAll<ILanguage>();
            DataObject = new Lexeme();
        }

        public AddEditDictionaryViewModel() : base(Enumerable.Empty<IDictata>())
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
            ValidWords = ConfigDataCache.GetAll<ILexeme>();
            ValidPhrases = ConfigDataCache.GetAll<IDictataPhrase>();
            ValidLanguages = ConfigDataCache.GetAll<ILanguage>();
            DataObject = new Lexeme();
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
        public IEnumerable<ILexeme> ValidWords { get; set; }
        public IEnumerable<IDictataPhrase> ValidPhrases { get; set; }
        public Lexeme DataObject { get; set; }
    }

    public class AddEditDictataViewModel : IBaseViewModel
    {
        public ApplicationUser AuthedUser { get; set; }

        public AddEditDictataViewModel()
        {
            ValidPhrases = ConfigDataCache.GetAll<IDictataPhrase>().OrderBy(word => word.Language.Name).ThenBy(word => word.Name);
            ValidLanguages = ConfigDataCache.GetAll<ILanguage>();
            DataObject = new Dictata();
        }

        public AddEditDictataViewModel(IDictata obj)
        {
            ValidPhrases = ConfigDataCache.GetAll<IDictataPhrase>().OrderBy(word => word.Language.Name).ThenBy(word => word.Name);
            ValidLanguages = ConfigDataCache.GetAll<ILanguage>();
            DataObject = (Dictata)obj;
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
        public IEnumerable<IDictataPhrase> ValidPhrases { get; set; }
        public Dictata DataObject { get; set; }
    }
}