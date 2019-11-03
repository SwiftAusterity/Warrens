using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.System;
using NetMud.Lexica.DeepLex;
using NetMud.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Text.RegularExpressions;
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
        private static readonly string wordNetTokenCacheKey = "WordNetEngine";
        private static readonly string mirriamWebsterTokenCacheKey = "MirriamHarness";

        public static WordNetEngine WordNet
        {
            get
            {
                return (WordNetEngine)globalCache[wordNetTokenCacheKey];
            }
            set
            {
                globalCache.AddOrGetExisting(wordNetTokenCacheKey, value, globalPolicy);
            }
        }

        public static MirriamWebsterHarness MirriamWebsterAPI
        {
            get
            {
                return (MirriamWebsterHarness)globalCache[mirriamWebsterTokenCacheKey];
            }
            set
            {
                globalCache.AddOrGetExisting(mirriamWebsterTokenCacheKey, value, globalPolicy);
            }
        }

        /// <summary>
        /// Map the synonyms of this
        /// </summary>
        /// <param name="dictata">the word in question</param>
        /// <returns>a trigger boolean to end a loop</returns>
        public static bool GetSynSet(IDictata dictata)
        {
            try
            {
                if (dictata.WordType != LexicalType.None)
                {
                    var wordList = new List<string>();
                    CreateOrModifyLexeme(dictata.Language, dictata.Name, dictata.WordType, ref wordList);
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
        /// Create or modify a lexeme with no word form basis, gets tricky with best fit scenarios
        /// </summary>
        /// <param name="word">just the text of the word</param>
        /// <returns>A lexeme</returns>
        public static ILexeme CreateOrModifyLexeme(ILanguage language, string word, LexicalType wordType, ref List<string> processedWords)
        {
            word = word.ToLower();

            Regex rgx = new Regex("[^a-z -]");
            word = rgx.Replace(word, "");

            if (string.IsNullOrWhiteSpace(word) || word.All(ch => ch == '-'))
            {
                return null;
            }

            ILexeme newLex = ConfigDataCache.Get<ILexeme>(string.Format("{0}_{1}_{2}", ConfigDataType.Dictionary, language.Name, word));

            if (newLex == null)
            {
                newLex = language.CreateOrModifyLexeme(word, wordType, new string[0]);
            }

            if (newLex.IsSynMapped || processedWords.Any(wrd => wrd.Equals(word)))
            {
                if (!processedWords.Any(wrd => wrd.Equals(word)))
                {
                    processedWords.Add(word);
                }

                return newLex;
            }

            LexicalType[] invalidTypes = new LexicalType[] { LexicalType.Article, LexicalType.Conjunction, LexicalType.ProperNoun, LexicalType.Pronoun, LexicalType.None };

            processedWords.Add(word);

            var newDict = newLex.GetForm(wordType, -1);

            //This is wordnet processing, wordnet doesnt have any of the above and will return weird results if we let it
            if (!invalidTypes.Contains(wordType))
            {
                bool exists = true;
                SearchSet searchSet = null;
                List<Search> results = new List<Search>();
                WordNet.OverviewFor(word, string.Empty, ref exists, ref searchSet, results);

                //We in theory have every single word form for this word now
                if (exists && results != null)
                {
                    foreach (SynonymSet synSet in results.Where(result => string.Compare(word, result.word, StringComparison.InvariantCultureIgnoreCase) == 0)
                                                         .SelectMany(result => result.senses).Where(sense => sense.words?.Count() > 0))
                    {
                        if (synSet.sstype == AdjSynSetType.IndirectAnt)
                            continue;

                        //grab semantics somehow
                        List<string> semantics = new List<string>();
                        var type = MapLexicalTypes(synSet.pos.Flag);

                        if (synSet.defn != null)
                        {
                            var indexSplit = synSet.defn.IndexOf(';');
                            string definition = synSet.defn.Substring(0, indexSplit < 0 ? synSet.defn.Length - 1 : indexSplit).Trim();
                            string[] defWords = definition.Split(' ');

                            foreach (string defWord in defWords)
                            {
                                var currentWord = defWord.ToLower();
                                currentWord = rgx.Replace(currentWord, "");

                                if (processedWords.Contains(currentWord))
                                    continue;

                                if (currentWord.Equals(word) || string.IsNullOrWhiteSpace(word) || word.All(ch => ch == '-') || word.IsNumeric())
                                {
                                    continue;
                                }

                                semantics.Add(currentWord);
                            }
                        }

                        ///wsns indicates hypo/hypernymity so
                        foreach (Lexeme synWord in synSet.words)
                        {
                            var newWord = synWord.word.ToLower();
                            newWord = rgx.Replace(newWord, "");

                            if (processedWords.Contains(newWord))
                                continue;

                            ///wsns indicates hypo/hypernymity so
                            int mySeverity = synWord.wnsns;
                            int myElegance = Math.Max(0, synWord.word.SyllableCount() * 3);
                            int myQuality = synWord.semcor?.semcor ?? 0;

                            //it's a phrase
                            if (synWord.word.Contains("_"))
                            {
                                string[] words = synWord.word.Split('_');

                                //foreach (string phraseWord in words)
                                //{
                                //    //make the phrase? maybe later
                                //}
                            }
                            else
                            {
                                processedWords.Add(newWord);

                                if (string.IsNullOrWhiteSpace(newWord) || newWord.All(ch => ch == '-') || newWord.IsNumeric())
                                {
                                    continue;
                                }

                                var synLex = language.CreateOrModifyLexeme(newWord, type, semantics.ToArray());

                                var synDict = synLex.GetForm(type, semantics.ToArray(), false);
                                synDict.Elegance = Math.Max(0, newDict.Name.SyllableCount() * 3);
                                synDict.Quality = synSet.words.Count();
                                synDict.Severity = synSet.words[Math.Max(0, synSet.whichword - 1)].wnsns;

                                synLex.PersistToCache();
                                synLex.SystemSave();

                                if (!newWord.Equals(word))
                                {
                                    newDict.MakeRelatedWord(language, newWord, synWord.wnsns > 0, synDict);
                                }
                            }
                        }
                    }
                }
            }

            newLex.IsSynMapped = true;
            newLex.SystemSave();
            newLex.PersistToCache();

            return newLex;
        }

        /// <summary>
        /// Verify the dictionary has this word already
        /// </summary>
        /// <param name="lexica">lexica to check</param>
        public static void VerifyLexeme(ILexica lexica)
        {
            if (lexica == null || string.IsNullOrWhiteSpace(lexica.Phrase) || lexica.Phrase.IsNumeric())
            {
                //we dont want numbers getting in the dict, thats bananas
                return;
            }

            VerifyLexeme(lexica.GetDictata().GetLexeme());
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

            ILexeme maybeLexeme = ConfigDataCache.Get<ILexeme>(string.Format("{0}_{1}_{2}", ConfigDataType.Dictionary, lexeme.Language, lexeme.Name));

            if (maybeLexeme != null)
            {
                lexeme = maybeLexeme;
            }

            lexeme.IsSynMapped = false;

            lexeme.PersistToCache();
            lexeme.SystemSave();

            lexeme.MapSynNet();
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

        public static void LoadMirriamHarness(string dictKey, string thesaurusKey)
        {
            MirriamWebsterAPI = new MirriamWebsterHarness(dictKey, thesaurusKey);
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
