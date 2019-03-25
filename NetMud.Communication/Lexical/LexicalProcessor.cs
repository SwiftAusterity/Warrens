using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.System;
using NetMud.Utility;
using System.Collections.Generic;
using System.Linq;

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
            if (string.IsNullOrWhiteSpace(lexica.Phrase) || lexica.Phrase.IsNumeric())
            {
                //we dont want numbers getting in the dict, thats bananas
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
            if (dictata == null || string.IsNullOrWhiteSpace(dictata.Name) || dictata.Name.IsNumeric())
            {
                return null;
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

            ConfigDataCacheKey cacheKey = new ConfigDataCacheKey(dictata);

            IDictata maybeDictata = ConfigDataCache.Get<IDictata>(cacheKey);

            if (maybeDictata != null)
            {
                if (dictata.WordTypes.Any(mWord => !maybeDictata.WordTypes.Contains(mWord)))
                {
                    var wordTypes = new HashSet<LexicalType>();
                    foreach (var wordType in dictata.WordTypes)
                    {
                        wordTypes.Add(wordType);
                    }

                    foreach (var wordType in maybeDictata.WordTypes.Where(mWord => !dictata.WordTypes.Contains(mWord)))
                    {
                        wordTypes.Add(wordType);
                    }

                    maybeDictata.WordTypes = wordTypes;
                }

                if (maybeDictata.Language != null)
                {
                    maybeDictata.FillLanguages();
                    maybeDictata.SystemSave();
                    maybeDictata.PersistToCache();

                    return maybeDictata;
                }

                dictata = maybeDictata;
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
