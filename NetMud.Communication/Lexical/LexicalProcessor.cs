using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.System;
using NetMud.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using WordNet.Net;
using WordNet.Net.Searching;
using WordNet.Net.WordNet;

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

        internal static WordNetEngine WordNet
        {
            get
            {
                return (WordNetEngine)globalCache[tokenCacheKey];
            }
            set
            {
                globalCache.AddOrGetExisting(tokenCacheKey, value, globalPolicy);
            }
        }

        public static bool GetSynSet(IDictata dictata)
        {
            try
            {
                if (MapLexicalTypes(dictata.WordType) != PartsOfSpeech.None)
                {
                    bool exists = true;
                    SearchSet searchSet = null;
                    List<Search> results = new List<Search>();
                    WordNet.OverviewFor(dictata.Name, MapLexicalTypes(dictata.WordType).ToString(), ref exists, ref searchSet, results);

                    //TODO: Do something with the results
                    if (exists && results != null)
                    {
                        LexicalType[] invalidTypes = new LexicalType[] { LexicalType.Article, LexicalType.Conjunction, LexicalType.ProperNoun, LexicalType.Pronoun, LexicalType.None };
                        //syn sets
                        foreach (SynonymSet synSet in results.SelectMany(result => result.senses))
                        {
                            //grab semantics somehow
                            List<string> semantics = new List<string>();
                            string definition = synSet.defn.Substring(0, synSet.defn.IndexOf(';')).Trim();
                            string[] defWords = definition.Split(' ');

                            foreach(string defWord in defWords)
                            {
                                ILexeme defLex = ConfigDataCache.Get<ILexeme>(string.Format("{0}_{1}", dictata.Language.Name, defWord));

                                if(defLex == null)
                                {
                                    //TODO: make the word
                                }

                                if(defLex == null || !defLex.ContainedTypes().Any(typ => invalidTypes.Contains(typ)))
                                {
                                    semantics.Add(defWord);
                                }
                            }

                            if(dictata.Semantics.Count() < semantics.Count())
                            {
                                semantics.AddRange(dictata.Semantics);
                                dictata.Semantics = new HashSet<string>(semantics.Distinct());
                            }

                            ///wsns indicates hypo/hypernymity so
                            int baseWeight = synSet.words[synSet.sense].wnsns;
                            dictata.Severity = baseWeight;

                            foreach (Lexeme word in synSet.words)
                            {
                                ///wsns indicates hypo/hypernymity so
                                int mySeverity = word.wnsns;

                                //Don't bother if this word is already the same word we started with
                                if (word.word.Equals(dictata.Name, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    continue;
                                }

                                //it's a phrase
                                if (word.word.Contains("_"))
                                {
                                    string[] words = word.word.Split('_');

                                    foreach (string phraseWord in words)
                                    {

                                    }
                                }
                                else
                                {
                                    dictata.MakeRelatedWord(dictata.Language, word.word, true);
                                }
                            }
                        }

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
                //don't barf on this
            }

            return false;
        }

        /// <summary>
        /// Verify the dictionary has this word already
        /// </summary>
        /// <param name="lexica">lexica to check</param>
        public static void VerifyLexeme(ILexica lexica)
        {
            if (string.IsNullOrWhiteSpace(lexica.Phrase) || lexica.Phrase.IsNumeric())
            {
                //we dont want numbers getting in the dict, thats bananas
                return;
            }

            //Experiment: make new everything
            if (VerifyLexeme(lexica.GetDictata().GetLexeme()) != null)
            {
                //make a new one
                lexica.GenerateDictata();
            }
        }

        /// <summary>
        /// Verify the dictionary has this word already
        /// </summary>
        /// <param name="lexeme">dictata to check</param>
        public static ILexeme VerifyLexeme(ILexeme lexeme)
        {
            if (lexeme == null || string.IsNullOrWhiteSpace(lexeme.Name) || lexeme.Name.IsNumeric())
            {
                return null;
            }

            //Set the language to default if it is absent and save it, if it has a language it already exists
            if (lexeme.Language == null)
            {
                IGlobalConfig globalConfig = ConfigDataCache.Get<IGlobalConfig>(new ConfigDataCacheKey(typeof(IGlobalConfig), "LiveSettings", ConfigDataType.GameWorld));

                if (globalConfig.BaseLanguage != null)
                {
                    lexeme.Language = globalConfig.BaseLanguage;
                }
            }

            ConfigDataCacheKey cacheKey = new ConfigDataCacheKey(lexeme);

            ILexeme maybeLexeme = ConfigDataCache.Get<ILexeme>(cacheKey);

            if (maybeLexeme != null)
            {
                lexeme = maybeLexeme;
            }

            lexeme.MapSynNet();
            lexeme.SystemSave();
            lexeme.PersistToCache();
            lexeme.FillLanguages();

            return lexeme;
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
            string wordNetPath = HttpContext.Current.Server.MapPath("/FileStore/wordnet/");

            if (!Directory.Exists(wordNetPath))
            {
                LoggingUtility.LogError(new FileNotFoundException("WordNet data not found."));
                return;
            }

            WordNet = new WordNetEngine(wordNetPath);
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
