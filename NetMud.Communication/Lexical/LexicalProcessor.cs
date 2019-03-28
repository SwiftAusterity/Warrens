using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.System;
using NetMud.Utility;
using Syn.WordNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace NetMud.Communication.Lexical
{
    /// <summary>
    /// Processes Lexica and outputs formatted prose
    /// </summary>
    public static class LexicalProcessor
    {
        private static WordNetEngine _wordNet;
        internal static WordNetEngine WordNet
        {
            get
            {
                if (_wordNet == null)
                {
                    LoadWordnet();
                }

                return _wordNet;
            }
            set
            {
                _wordNet = value;
            }
        }

        public static SynSet GetSynSet(IDictata dictata, LexicalType specificType)
        {
            SynSet synSet = null;

            var synType = MapLexicalTypes(specificType);
            if (synType == PartOfSpeech.None || dictata?.Language == null || !dictata.Language.SuitableForUse)
            {
                return synSet;
            }

            synSet = WordNet.GetMostCommonSynSet(dictata.Name, synType);

            if (synSet != null && !dictata.WordTypes.Contains(specificType))
            {
                var wordTypes = new HashSet<LexicalType>(dictata.WordTypes)
                {
                    specificType
                };

                dictata.WordTypes = wordTypes;
                dictata.SystemSave();
                dictata.PersistToCache();
                dictata.FillLanguages();
            }

            return synSet;
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
            var _wordNet = new WordNetEngine();

            try
            {
                var directory = HttpContext.Current.Server.MapPath("/FileStore/wordnet/");

                _wordNet.AddDataSource(new StreamReader(Path.Combine(directory, "data.adj")), PartOfSpeech.Adjective);
                _wordNet.AddDataSource(new StreamReader(Path.Combine(directory, "data.adv")), PartOfSpeech.Adverb);
                _wordNet.AddDataSource(new StreamReader(Path.Combine(directory, "data.noun")), PartOfSpeech.Noun);
                _wordNet.AddDataSource(new StreamReader(Path.Combine(directory, "data.verb")), PartOfSpeech.Verb);

                _wordNet.AddIndexSource(new StreamReader(Path.Combine(directory, "index.adj")), PartOfSpeech.Adjective);
                _wordNet.AddIndexSource(new StreamReader(Path.Combine(directory, "index.adv")), PartOfSpeech.Adverb);
                _wordNet.AddIndexSource(new StreamReader(Path.Combine(directory, "index.noun")), PartOfSpeech.Noun);
                _wordNet.AddIndexSource(new StreamReader(Path.Combine(directory, "index.verb")), PartOfSpeech.Verb);

                _wordNet.Load();
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
                _wordNet = null;
            }
        }

        public static LexicalType MapLexicalTypes(PartOfSpeech pos)
        {
            switch (pos)
            {
                case PartOfSpeech.Adjective:
                    return LexicalType.Adjective;
                case PartOfSpeech.Adverb:
                    return LexicalType.Adverb;
                case PartOfSpeech.Noun:
                    return LexicalType.Noun;
                case PartOfSpeech.Verb:
                    return LexicalType.Verb;
            }

            return LexicalType.None;
        }

        public static PartOfSpeech MapLexicalTypes(LexicalType pos)
        {
            switch (pos)
            {
                case LexicalType.Adjective:
                    return PartOfSpeech.Adjective;
                case LexicalType.Adverb:
                    return PartOfSpeech.Adverb;
                case LexicalType.Noun:
                    return PartOfSpeech.Noun;
                case LexicalType.Verb:
                    return PartOfSpeech.Verb;
            }

            return PartOfSpeech.None;
        }
    }
}
