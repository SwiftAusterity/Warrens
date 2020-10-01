using NetMud.Backup;
using NetMud.CentralControl;
using NetMud.Communication.Lexical;
using NetMud.Data.Linguistic;
using NetMud.Data.System;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.System;
using System;
using System.Linq;
using System.Reflection;
using System.Web;

namespace NetMud
{
    public class GameConfig
    {
        public static void PreloadSupportingEntities()
        {
            //Load the "config" data first
            ConfigData.LoadEverythingToCache();

            LexicalProcessor.LoadWordnet();

            IGlobalConfig globalConfig = ConfigDataCache.Get<IGlobalConfig>(new ConfigDataCacheKey(typeof(IGlobalConfig), "LiveSettings", ConfigDataType.GameWorld));

            //We dont move forward without a global config
            if (globalConfig == null)
            {
                globalConfig = new GlobalConfig();

                globalConfig.SystemSave();
            }

            if (globalConfig.BaseLanguage == null)
            {
                ILanguage baseLanguage = ConfigDataCache.GetAll<ILanguage>().FirstOrDefault();

                if (baseLanguage == null)
                {
                    LoggingUtility.Log("There are no valid languages. Generating new base language.", LogChannels.SystemErrors, true);

                    baseLanguage = new Language()
                    {
                        Name = "English",
                        GoogleLanguageCode = "en-us",
                        AntecendentPunctuation = true,
                        PrecedentPunctuation = false,
                        Gendered = false,
                        UIOnly = true
                    };

                    baseLanguage.SystemSave();
                }

                globalConfig.BaseLanguage = baseLanguage;
                globalConfig.SystemSave();
            }

            if (globalConfig.DeepLexActive)
            {
                LexicalProcessor.LoadMirriamHarness(globalConfig.MirriamDictionaryKey, globalConfig.MirriamThesaurusKey);

                //start the deep lexer (which runs syn mapping, then translate, then mirriam mapper)
                Func<bool> deepLexer = LexicalProcessor.DeepLexer;

                //every 5 seconds after 30
                Processor.StartSingeltonChainedLoop("DeepLexer", 30, 30, -1, deepLexer);

                //start the lex cleanup thread
                Func<bool> lexJanitor = LexicalProcessor.LexicalJanitor;

                //every 15 mins after an hour
                Processor.StartSingeltonChainedLoop("LexicalJanitor", 60 * 60, 60 * 15, -1, lexJanitor);
            }

            //Ensure we have base words for the language every time
            globalConfig.BaseLanguage.SystemSave();

            HttpApplication instance = HttpContext.Current.ApplicationInstance;
            Assembly asm = instance.GetType().BaseType.Assembly;
            Version v = asm.GetName().Version;

            //Load structural data next
            Templates.LoadEverythingToCache();

            Func<bool> backingDataBackupFunction = Templates.WriteFullBackup;

            //every 2 hours after 1 hour
            Processor.StartSingeltonChainedLoop("BackingDataFullBackup", 60 * 60, 120 * 60, -1, backingDataBackupFunction);

        }
    }
}