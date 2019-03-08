using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.System;

namespace NetMud.Communication.Lexical
{
    /// <summary>
    /// Processes Lexica and outputs formatted prose
    /// </summary>
    public static class LexicalProcessor
    {
        /// <summary>
        /// Verify the dictionary has this word already
        /// </summary>
        /// <param name="lexica">lexica to check</param>
        public static void VerifyDictata(ILexica lexica)
        {
            if (string.IsNullOrWhiteSpace(lexica.Phrase))
            {
                return;
            }

            //Experiment: make new everything
            if (VerifyDictata(lexica.GetDictata()) != null)
            {
                //make a new one
                lexica.GenerateDictata();
            }
        }

        /// <summary>
        /// Verify the dictionary has this word already
        /// </summary>
        /// <param name="dictata">dictata to check</param>
        public static IDictata VerifyDictata(IDictata dictata)
        {
            if (dictata == null || string.IsNullOrWhiteSpace(dictata.Name))
            {
                return null;
            }

            ConfigDataCacheKey cacheKey = new ConfigDataCacheKey(dictata);

            IDictata maybeDictata = ConfigDataCache.Get<IDictata>(cacheKey);

            if (maybeDictata != null)
            {
                if (maybeDictata.Language != null)
                {
                    maybeDictata.FillLanguages();
                    return dictata;
                }

                dictata = maybeDictata;
            }

            //Set the language to default if it is absent and save it, if it has a language it already exists
            if (dictata.Language == null)
            {
                IGlobalConfig globalConfig = ConfigDataCache.Get<IGlobalConfig>(new ConfigDataCacheKey(typeof(IGlobalConfig), "LiveSettings", ConfigDataType.GameWorld));

                if (globalConfig.BaseLanguage != null)
                {
                    dictata.Language = globalConfig.BaseLanguage;
                }
            }

            dictata.SystemSave();
            dictata.PersistToCache();
            dictata.FillLanguages();

            return dictata;
        }

        public static string GetPunctuationMark(SentenceType type)
        {
            string punctuation = string.Empty;
            switch (type)
            {
                case SentenceType.Exclamation:
                    punctuation = "!";
                    break;
                case SentenceType.ExclamitoryQuestion:
                    punctuation = "?!";
                    break;
                case SentenceType.Partial:
                    punctuation = ";";
                    break;
                case SentenceType.Question:
                    punctuation = "?";
                    break;
                case SentenceType.Statement:
                case SentenceType.None:
                    punctuation = ".";
                    break;
            }

            return punctuation;
        }
    }
}
