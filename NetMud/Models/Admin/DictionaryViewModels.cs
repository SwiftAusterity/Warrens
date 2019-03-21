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
    public class ManageDictionaryViewModel : PagedDataModel<IDictata>
    {
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

    public class AddEditDictionaryViewModel : AddEditConfigDataModel<IDictata>
    {
        [Display(Name = "Apply Existing Template", Description = "Apply an existing object's data to this new data.")]
        [UIHint("DictataList")]
        [DictataDataBinder]
        public override IDictata Template { get; set; }

        public AddEditDictionaryViewModel() : base("", ConfigDataType.Dictionary)
        {
            ValidWords = ConfigDataCache.GetAll<IDictata>();
            ValidPhrases = ConfigDataCache.GetAll<IDictataPhrase>();
            ValidLanguages = ConfigDataCache.GetAll<ILanguage>();
            DataObject = new Dictata();
        }

        public AddEditDictionaryViewModel(string uniqueKey) : base(uniqueKey, ConfigDataType.Dictionary)
        {
            ValidWords = ConfigDataCache.GetAll<IDictata>();
            ValidPhrases = ConfigDataCache.GetAll<IDictataPhrase>();
            ValidLanguages = ConfigDataCache.GetAll<ILanguage>();
            DataObject = new Dictata();

            //apply template
            if (DataTemplate != null)
            {
                DataObject.Antonyms = DataTemplate.Antonyms;
                DataObject.Determinant = DataTemplate.Determinant;
                DataObject.Elegance = DataTemplate.Elegance;
                DataObject.Feminine = DataTemplate.Feminine;
                DataObject.Language = DataTemplate.Language;
                DataObject.Perspective = DataTemplate.Perspective;
                DataObject.Plural = DataTemplate.Plural;
                DataObject.Positional = DataTemplate.Positional;
                DataObject.Possessive = DataTemplate.Possessive;
                DataObject.Quality = DataTemplate.Quality;
                DataObject.Semantics = DataTemplate.Semantics;
                DataObject.Severity = DataTemplate.Severity;
                DataObject.Synonyms = DataTemplate.Synonyms;
                DataObject.Tense = DataTemplate.Tense;
                DataObject.WordTypes = DataTemplate.WordTypes;
            }
        }

        public AddEditDictionaryViewModel(string archivePath, IDictata item) : base(archivePath, ConfigDataType.Dictionary, item)
        {
            ValidWords = ConfigDataCache.GetAll<IDictata>();
            ValidPhrases = ConfigDataCache.GetAll<IDictataPhrase>();
            ValidLanguages = ConfigDataCache.GetAll<ILanguage>();
            DataObject = (Dictata)item;
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