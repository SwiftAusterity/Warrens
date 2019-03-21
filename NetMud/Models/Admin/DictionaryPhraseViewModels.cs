using NetMud.Data.Architectural.PropertyBinding;
using NetMud.Data.Linguistic;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Linguistic;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.Admin
{
    public class ManageDictionaryPhraseViewModel : PagedDataModel<IDictataPhrase>
    {
        public ManageDictionaryPhraseViewModel(IEnumerable<IDictataPhrase> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IDictataPhrase, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }

        internal override Func<IDictataPhrase, object> OrderPrimary
        {
            get
            {
                return item => item.Language.Name;
            }
        }


        internal override Func<IDictataPhrase, object> OrderSecondary
        {
            get
            {
                return item => item.Name;
            }
        }
    }

    public class AddEditDictionaryPhraseViewModel : AddEditConfigDataModel<IDictataPhrase>
    {
        [Display(Name = "Apply Existing Template", Description = "Apply an existing object's data to this new data.")]
        [UIHint("DictataPhraseList")]
        [DictataPhraseDataBinder]
        public override IDictataPhrase Template { get; set; }

        public AddEditDictionaryPhraseViewModel() : base("", ConfigDataType.Dictionary)
        {
            ValidWords = ConfigDataCache.GetAll<IDictataPhrase>();
            ValidPhrases = ConfigDataCache.GetAll<IDictataPhrase>();
            ValidLanguages = ConfigDataCache.GetAll<ILanguage>();
            DataObject = new DictataPhrase();
        }

        public AddEditDictionaryPhraseViewModel(string uniqueKey) : base(uniqueKey, ConfigDataType.Dictionary)
        {
            ValidWords = ConfigDataCache.GetAll<IDictataPhrase>();
            ValidPhrases = ConfigDataCache.GetAll<IDictataPhrase>();
            ValidLanguages = ConfigDataCache.GetAll<ILanguage>();
            DataObject = new DictataPhrase();

            //apply template
            if (DataTemplate != null)
            {
                DataObject.Antonyms = DataTemplate.Antonyms;
                DataObject.Elegance = DataTemplate.Elegance;
                DataObject.Feminine = DataTemplate.Feminine;
                DataObject.Language = DataTemplate.Language;
                DataObject.Perspective = DataTemplate.Perspective;
                DataObject.Positional = DataTemplate.Positional;
                DataObject.Quality = DataTemplate.Quality;
                DataObject.Semantics = DataTemplate.Semantics;
                DataObject.Severity = DataTemplate.Severity;
                DataObject.Synonyms = DataTemplate.Synonyms;
                DataObject.Tense = DataTemplate.Tense;
            }
        }

        public AddEditDictionaryPhraseViewModel(string archivePath, IDictataPhrase item) : base(archivePath, ConfigDataType.Dictionary, item)
        {
            ValidWords = ConfigDataCache.GetAll<IDictataPhrase>();
            ValidPhrases = ConfigDataCache.GetAll<IDictataPhrase>();
            ValidLanguages = ConfigDataCache.GetAll<ILanguage>();
            DataObject = (DictataPhrase)item;
        }

        public IEnumerable<ILanguage> ValidLanguages { get; set; }
        public IEnumerable<IDictataPhrase> ValidWords { get; set; }
        public IEnumerable<IDictataPhrase> ValidPhrases { get; set; }
        public DictataPhrase DataObject { get; set; }
    }
}