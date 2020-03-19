using NetMud.Authentication;
using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;

namespace NetMud.Models.Admin
{
    public class DashboardViewModel : IBaseViewModel
    {
        public ApplicationUser AuthedUser { get; set; }

        public DashboardViewModel()
        {
            HelpFiles = Enumerable.Empty<IHelp>();
            Journals = Enumerable.Empty<IJournalEntry>();

            DictionaryWords = Enumerable.Empty<ILexeme>();
            Languages = Enumerable.Empty<ILanguage>();
        }

        public IEnumerable<IHelp> HelpFiles { get; set; }
        public IEnumerable<IJournalEntry> Journals { get; set; }

        //Config Data
        public IEnumerable<ILexeme> DictionaryWords { get; set; }
        public IEnumerable<ILanguage> Languages { get; set; }

        //Running Data
        public Dictionary<string, CancellationTokenSource> LiveTaskTokens { get; set; }

        [Display(Name = "System Language", Description = "The default language used for the system.")]
        [DataType(DataType.Text)]
        public string SystemLanguage { get; set; }

        [Display(Name = "User Creation", Description = "Are new users allowed to register?")]
        [UIHint("Boolean")]
        public bool UserCreationActive { get; set; }

        [Display(Name = "Admins Only", Description = "Are only admins allowed to log in - noone at StaffRank.Player?")]
        [UIHint("Boolean")]
        public bool AdminsOnly { get; set; }

        [Display(Name = "Live Translation", Description = "Do new Dictata get translated to the UI languages?")]
        [UIHint("Boolean")]
        public bool TranslationActive { get; set; }

        [Display(Name = "Azure API Key", Description = "The API key for your azure translation service.")]
        [DataType(DataType.Text)]
        public string AzureTranslationKey { get; set; }

        [Display(Name = "Deep Lex", Description = "Do words get deep lexed through Mirriam Webster?")]
        [UIHint("Boolean")]
        public bool DeepLexActive { get; set; }

        [Display(Name = "Mirriam Dictionary Key", Description = "The API key for your mirriam webster dictionary service.")]
        [DataType(DataType.Text)]
        public string MirriamDictionaryKey { get; set; }

        [Display(Name = "Mirriam Thesaurus Key", Description = "The API key for your mirriam webster thesaurus service.")]
        [DataType(DataType.Text)]
        public string MirriamThesaurusKey { get; set; }

        [Display(Name = "Base Language", Description = "The base language for the system.")]
        [UIHint("LanguageList")]
        [LanguageDataBinder]
        public ILanguage BaseLanguage { get; set; }

        [Display(Name = "Retry Loop Maximum", Description = "The maximum retry value. Higher = more retries.")]
        [Range(200, 1000, ErrorMessage = "Must be between 200 and 1000.")]
        [DataType(DataType.Text)]
        public double SuspendMultiplierMaximum { get; set; }

        [Display(Name = "Retry Loop Multiplier", Description = "How much the retry loop monitor escalates each loop. Higher = less retries.")]
        [Range(1.15, 3, ErrorMessage = "Must be between 1.15 and 3.")]
        [DataType(DataType.Text)]
        public double SuspendMultiplier { get; set; }

        [Display(Name = "Backup Name", Description = "Include a name for this backup to make it a permenant archival point.")]
        [DataType(DataType.Text)]
        public string BackupName { get; set; }

        public IEnumerable<ILanguage> ValidLanguages { get; set; }
        public IGlobalConfig ConfigDataObject { get; set; }
    }

    public class GlobalConfigViewModel
    {
        public ApplicationUser authedUser { get; set; }

        [Display(Name = "System Language", Description = "The default language used for the system.")]
        [DataType(DataType.Text)]
        public string SystemLanguage { get; set; }

        public IEnumerable<ILanguage> ValidLanguages { get; set; }
        public IGlobalConfig DataObject { get; set; }
    }
}