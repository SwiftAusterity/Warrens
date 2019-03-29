using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.System;
using NetMud.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using WordNet.Net.Searching;

namespace NetMud.Communication.Lexical
{
    /// <summary>
    /// Processes Lexica and outputs formatted prose
    /// </summary>
    public static class LexicalProcessor
    {
        private static readonly ObjectCache globalCache = MemoryCache.Default;
        private static readonly CacheItemPolicy globalPolicy = new CacheItemPolicy();
        private static readonly string tokenCacheKey = "WordNetEngine";

        public static void GetSynSet(IDictata dictata, LexicalType specificType)
        {
        }

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
                    dictata.MapSynNet();
                    maybeDictata.FillLanguages();
                    maybeDictata.SystemSave();
                    maybeDictata.PersistToCache();

                    return maybeDictata;
                }

                dictata = maybeDictata;
            }

            dictata.MapSynNet();
            dictata.SystemSave();
            dictata.PersistToCache();
            dictata.FillLanguages();

            return dictata;
        }

        public static string GetPunctuationMark(SentenceType type, bool upsideDown = false)
        {
            string punctuation = string.Empty;
            switch (type)
            {
                case SentenceType.Exclamation:
                    punctuation = upsideDown ? "!" : "!";
                    break;
                case SentenceType.ExclamitoryQuestion:
                    punctuation = upsideDown ? "?!" : "?!";
                    break;
                case SentenceType.Partial:
                    punctuation = ";";
                    break;
                case SentenceType.Question:
                    punctuation = upsideDown ? "?" : "?";
                    break;
                case SentenceType.Statement:
                case SentenceType.None:
                    punctuation = ".";
                    break;
            }

            return punctuation;
        }

        public static void LoadWordnet()
        {

        }

        public static LexicalType MapLexicalTypes(PartsOfSpeech pos)
        {
            switch (pos)
            {
                case PartsOfSpeech.Adjective:
                    return LexicalType.Adjective;
                case PartsOfSpeech.Adverb:
                    return LexicalType.Adverb;
                case PartsOfSpeech.Noun:
                    return LexicalType.Noun;
                case PartsOfSpeech.Verb:
                    return LexicalType.Verb;
            }

            return LexicalType.None;
        }

        public static PartsOfSpeech MapLexicalTypes(LexicalType pos)
        {
            switch (pos)
            {
                case LexicalType.Adjective:
                    return PartsOfSpeech.Adjective;
                case LexicalType.Adverb:
                    return PartsOfSpeech.Adverb;
                case LexicalType.Noun:
                    return PartsOfSpeech.Noun;
                case LexicalType.Verb:
                    return PartsOfSpeech.Verb;
            }

            return PartsOfSpeech.None;
        }
    }
}
