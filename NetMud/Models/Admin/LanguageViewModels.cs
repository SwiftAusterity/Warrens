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
    public class ManageLanguageDataViewModel : PagedDataModel<ILanguage>
    {
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

        internal override Func<ILanguage, object> OrderPrimary
        {
            get
            {
                return item => item.UIOnly ? 0 : 1;
            }
        }


        internal override Func<ILanguage, object> OrderSecondary
        {
            get
            {
                return item => item.Name;
            }
        }
    }

    public class AddEditLanguageViewModel : AddEditConfigDataModel<ILanguage>
    {
        [Display(Name = "Apply Existing Template", Description = "Apply an existing object's data to this new data.")]
        [UIHint("LanguageList")]
        [LanguageDataBinder]
        public override ILanguage Template { get; set; }

        public AddEditLanguageViewModel() : base("", ConfigDataType.Language)
        {
            ValidWords = ConfigDataCache.GetAll<ILexeme>().OrderBy(word => word.Language.Name).ThenBy(word => word.Name);
            ValidLanguages = ConfigDataCache.GetAll<ILanguage>();
            DataObject = new Language();
        }

        public AddEditLanguageViewModel(string uniqueKey) : base(uniqueKey, ConfigDataType.Language)
        {
            ValidWords = ConfigDataCache.GetAll<ILexeme>().OrderBy(word => word.Language.Name).ThenBy(word => word.Name);
            ValidLanguages = ConfigDataCache.GetAll<ILanguage>();
            DataObject = new Language();

            //apply template
            if (DataTemplate != null)
            {
                DataObject.AntecendentPunctuation = DataTemplate.AntecendentPunctuation;
                DataObject.BaseWords = DataTemplate.BaseWords;
                DataObject.ContractionRules = DataTemplate.ContractionRules;
                DataObject.TransformationRules = DataTemplate.TransformationRules;
                DataObject.Gendered = DataTemplate.Gendered;
                DataObject.PrecedentPunctuation = DataTemplate.PrecedentPunctuation;
                DataObject.WordPairRules = DataTemplate.WordPairRules;
                DataObject.WordRules = DataTemplate.WordRules;
                DataObject.SentenceRules = DataTemplate.SentenceRules;
                DataObject.UIOnly = DataTemplate.UIOnly;
            }
        }

        public AddEditLanguageViewModel(string archivePath, ILanguage item) : base(archivePath, ConfigDataType.Language, item)
        {
            ValidWords = ConfigDataCache.GetAll<ILexeme>().Where(word => word.Language == item).OrderBy(word => word.Name);
            ValidLanguages = ConfigDataCache.GetAll<ILanguage>();
            DataObject = (Language)item;
        }

        public IEnumerable<ILanguage> ValidLanguages { get; set; }
        public IEnumerable<ILexeme> ValidWords { get; set; }
        public Language DataObject { get; set; }
    }
}
