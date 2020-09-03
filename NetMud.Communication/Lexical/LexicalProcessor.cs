using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.System;
using NetMud.Lexica.DeepLex;
using NetMud.Utility;
using Syn.WordNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using System.Text.RegularExpressions;
using System.Web;

namespace NetMud.Communication.Lexical
{
    /// <summary>
    /// Processes Lexica and outputs formatted prose
    /// </summary>
    public static class LexicalProcessor
    {
        private static readonly ObjectCache globalCache = MemoryCache.Default;
        private static readonly CacheItemPolicy globalPolicy = new CacheItemPolicy();
        private static readonly string wordNetTokenCacheKey = "WordNetHarness";
        private static readonly string mirriamWebsterTokenCacheKey = "MirriamHarness";

        public static WordNetEngine WordNetHarness
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

            if (rgx.IsMatch(word))
            {
                return null;
            }

            ILexeme newLex = ConfigDataCache.Get<ILexeme>(string.Format("{0}_{1}_{2}", ConfigDataType.Dictionary, language.Name, word));

            if (newLex == null)
            {
                newLex = language.CreateOrModifyLexeme(word, wordType, new string[0]);
            }

            if ((newLex.IsSynMapped && newLex.MirriamIndexed) || processedWords.Any(wrd => wrd.Equals(word)))
            {
                if (!processedWords.Any(wrd => wrd.Equals(word)))
                {
                    processedWords.Add(word);
                }
            }
            else
            {
                LexicalType[] invalidTypes = new LexicalType[] { LexicalType.Article, LexicalType.Conjunction, LexicalType.ProperNoun, LexicalType.Pronoun, LexicalType.None };

                processedWords.Add(word);

                //This is wordnet processing, wordnet doesnt have any of the above and will return weird results if we let it
                if (!invalidTypes.Contains(wordType))
                {
                    var synSets = WordNetHarness.GetSynSets(word, new PartOfSpeech[] { PartOfSpeech.Adjective, PartOfSpeech.Adverb, PartOfSpeech.Noun, PartOfSpeech.Verb });

                    //We in theory have every single word form for this word now
                    if (synSets != null)
                    {
                        SemanticContext[] invalidContexts = new SemanticContext[]
                            { SemanticContext.Group, SemanticContext.Event, SemanticContext.Location, SemanticContext.Competition, SemanticContext.Person
                            , SemanticContext.Plant, SemanticContext.Animal, SemanticContext.Time, SemanticContext.Artifact };

                        foreach (SynSet synSet in synSets)
                        {
                            if (synSet.PartOfSpeech == PartOfSpeech.None)
                                continue;

                            var synContext = TranslateContext(synSet.LexicographerFileName);

                            if (invalidContexts.Contains(synContext))
                                continue;

                            var lexType = MapLexicalTypes(synSet.PartOfSpeech);
                            var newDict = newLex.GetForm(lexType, -1);

                            if (newDict == null)
                            {
                                newLex = language.CreateOrModifyLexeme(word, lexType, new string[0]);
                                newDict = newLex.GetForm(lexType, -1);
                                newDict.Context = TranslateContext(synSet.LexicographerFileName);
                            }

                            //We're going to use the definition from here
                            if (!string.IsNullOrWhiteSpace(synSet.Gloss))
                            {
                                newDict.Definition = synSet.Gloss;
                            }

                            var semantics = newDict.Semantics.ToArray();

                            ///wsns indicates hypo/hypernymity so
                            foreach (string synWord in synSet.Words)
                            {
                                MakeRelatedWord(synWord, word, newDict, rgx, processedWords, language, lexType, semantics, true, false, false);
                            }
                        }
                    }
                }

                newLex.IsSynMapped = true;
                newLex.SystemSave();
                newLex.PersistToCache();
            }

            if (!newLex.MirriamIndexed)
            {
                var newDict = newLex.GetForm(-1);
                if (string.IsNullOrEmpty(newDict.Definition))
                {
                    newDict.Definition = "";
                }

                try
                {
                    var dictEntry = MirriamWebsterAPI.GetDictionaryEntry(newLex.Name);
                    if (dictEntry != null)
                    {
                        if (dictEntry.shortdef != null && dictEntry.shortdef.Any())
                        {
                            newDict.Definition = " * " + newDict.Definition + " * " + string.Join(" * ", dictEntry.shortdef);
                        }

                        //Stuff done to modify all forms of the lexeme
                        foreach (var dict in newLex.WordForms)
                        {
                            dict.Vulgar = dictEntry.meta.offensive;
                        }

                        if (dictEntry.hwi != null)
                        {
                            var lexTypeString = dictEntry.fl;
                            var pluralize = false;
                            var definitive = false;
                            if (lexTypeString.StartsWith("plural"))
                            {
                                lexTypeString = lexTypeString.Substring(6);
                                pluralize = true;
                            }

                            if (lexTypeString.StartsWith("definite"))
                            {
                                lexTypeString = lexTypeString.Substring(8).Trim();
                                definitive = true;
                            }

                            var lexicalType = MapLexicalTypes(lexTypeString);

                            newDict.Plural = pluralize;
                            newDict.Determinant = definitive;
                            newLex.Phonetics = dictEntry.hwi.prs?.FirstOrDefault()?.mw;
                            newDict.Vulgar = dictEntry.meta.offensive;
                        }

                        //Stuff done based on the dictionary return data
                        foreach (var stemWord in dictEntry.uros)
                        {
                            var lexTypeString = stemWord.fl;
                            var pluralize = false;
                            var definitive = false;
                            if (lexTypeString.StartsWith("plural"))
                            {
                                lexTypeString = lexTypeString.Substring(6);
                                pluralize = true;
                            }

                            if (lexTypeString.StartsWith("definite"))
                            {
                                lexTypeString = lexTypeString.Substring(8).Trim();
                                definitive = true;
                            }

                            var lexicalType = MapLexicalTypes(lexTypeString);
                            if (newLex.GetForm(lexicalType) == null)
                            {
                                var wordText = stemWord.ure.Replace("*", "");
                                ILexeme stemLex = ConfigDataCache.Get<ILexeme>(string.Format("{0}_{1}_{2}", ConfigDataType.Dictionary, language.Name, wordText));

                                if (stemLex == null)
                                {
                                    stemLex = language.CreateOrModifyLexeme(wordText, lexicalType, null);
                                    stemLex.Phonetics = stemWord.prs?.FirstOrDefault()?.mw;

                                    var stemDict = stemLex.GetForm(0);
                                    stemDict.Elegance = newDict.Elegance;
                                    stemDict.Quality = newDict.Quality;
                                    stemDict.Severity = newDict.Severity;
                                    stemDict.Context = newDict.Context;
                                    stemDict.Definition = newDict.Definition;
                                    stemDict.Plural = pluralize;
                                    stemDict.Determinant = definitive;
                                    stemDict.Semantics = new HashSet<string>(newDict.Semantics.Where(word => !string.Equals(word, "system_command", StringComparison.InvariantCultureIgnoreCase)));
                                    processedWords.Add(wordText);

                                    stemLex.SystemSave();
                                    stemLex.PersistToCache();
                                }
                            }
                        }

                        newDict.Semantics = new HashSet<string>(dictEntry.sls);
                    }
                }
                catch
                {
                    //just eating it
                }

                try
                {
                    var thesEntry = MirriamWebsterAPI.GetThesaurusEntry(newLex.Name);
                    if (thesEntry != null)
                    {
                        var lexTypeString = thesEntry.fl;
                        var pluralize = false;
                        var definitive = false;
                        if (lexTypeString.StartsWith("plural"))
                        {
                            lexTypeString = lexTypeString.Substring(6).Trim();
                            pluralize = true;
                        }

                        if (lexTypeString.StartsWith("definite"))
                        {
                            lexTypeString = lexTypeString.Substring(8).Trim();
                            definitive = true;
                        }

                        var lexType = MapLexicalTypes(lexTypeString);

                        if (lexType != LexicalType.None)
                        {
                            var semantics = newDict.Semantics.ToArray();

                            foreach (var synonym in thesEntry.meta.syns.SelectMany(syn => syn))
                            {
                                MakeRelatedWord(synonym, word, newDict, rgx, processedWords, language, lexType, semantics, true, pluralize, definitive);
                            }

                            foreach (var antonym in thesEntry.meta.ants.SelectMany(syn => syn))
                            {
                                MakeRelatedWord(antonym, word, newDict, rgx, processedWords, language, lexType, semantics, false, pluralize, definitive);
                            }
                        }
                    }
                }
                catch
                {
                    //just eating it
                }

                newLex.MirriamIndexed = true;
                newLex.SystemSave();
                newLex.PersistToCache();
            }

            if (!newLex.IsTranslated)
            {

            }

            return newLex;
        }

        private static void MakeRelatedWord(string possibleWord, string word, IDictata newDict, Regex rgx, List<string> processedWords, ILanguage language,
            LexicalType lexType, string[] semantics, bool synonym, bool plural, bool definitive)
        {
            var newWord = possibleWord.ToLower();
            newWord = newWord.Replace("_", " ");

            if (rgx.IsMatch(newWord) || string.IsNullOrWhiteSpace(newWord) || newWord.All(ch => ch == '-') || newWord.IsNumeric())
                return;

            var validSemantics = semantics.Where(word => !string.Equals(word, "system_command", StringComparison.InvariantCultureIgnoreCase)).ToArray();

            var synLex = language.CreateOrModifyLexeme(newWord, lexType, validSemantics);
            var synDict = synLex.GetForm(lexType, validSemantics, false);

            synDict.Elegance = 0;
            synDict.Quality = 0;
            synDict.Severity = 0;
            synDict.Context = newDict.Context;
            synDict.Definition = newDict.Definition;
            synDict.Plural = plural;
            synDict.Determinant = definitive;

            synLex.PersistToCache();
            synLex.SystemSave();
            processedWords.Add(newWord);

            if (!newWord.Equals(word))
            {
                newDict.MakeRelatedWord(language, newWord, synonym, synDict);
            }

            if (!string.IsNullOrWhiteSpace(newDict.Definition))
            {
                //experimental
                foreach (var defWord in newDict.Definition.Split(' '))
                {
                    VeryDeepLex(language, defWord);
                }
            }
        }

        private static void VeryDeepLex(ILanguage language, string word)
        {
            language.CreateOrModifyLexeme(word, LexicalType.None, new string[0]);
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

            var engine = new WordNetEngine();
            engine.LoadFromDirectory(wordNetPath);
            WordNetHarness = engine;
        }

        public static void LoadMirriamHarness(string dictKey, string thesaurusKey)
        {
            MirriamWebsterAPI = new MirriamWebsterHarness(dictKey, thesaurusKey);
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
                case PartOfSpeech.None:
                    break;
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
                case LexicalType.None:
                    break;
                case LexicalType.Pronoun:
                    break;
                case LexicalType.Conjunction:
                    break;
                case LexicalType.Interjection:
                    break;
                case LexicalType.ProperNoun:
                    break;
                case LexicalType.Article:
                    break;
                case LexicalType.Preposition:
                    break;
            }

            return PartOfSpeech.None;
        }

        public static LexicalType MapLexicalTypes(string fl)
        {
            switch (fl.Trim())
            {
                case "adjective":
                    return LexicalType.Adjective;
                case "adverb":
                    return LexicalType.Adverb;
                case "noun":
                    return LexicalType.Noun;
                case "verb":
                    return LexicalType.Verb;
                case "preposition":
                    return LexicalType.Preposition;
                case "phrase":
                    return LexicalType.Interjection;
                case "article":
                    return LexicalType.Article;
                case "name":
                    return LexicalType.ProperNoun;
                case "pronoun":
                    return LexicalType.Pronoun;
                default:
                    break;
            }

            return LexicalType.None;
        }

        public static SemanticContext TranslateContext(LexicographerFileName fileName)
        {
            SemanticContext context = SemanticContext.None;

            switch (fileName)
            {
                case LexicographerFileName.NounAct:
                    context = SemanticContext.Act;
                    break;
                case LexicographerFileName.NounAnimal:
                    context = SemanticContext.Animal;
                    break;
                case LexicographerFileName.NounArtifact:
                    context = SemanticContext.Artifact;
                    break;
                case LexicographerFileName.NounAttribute:
                    context = SemanticContext.Attribute;
                    break;
                case LexicographerFileName.NounBody:
                    context = SemanticContext.Body;
                    break;
                case LexicographerFileName.NounCognition:
                    context = SemanticContext.Cognition;
                    break;
                case LexicographerFileName.NounCommunication:
                    context = SemanticContext.Communication;
                    break;
                case LexicographerFileName.NounEvent:
                    context = SemanticContext.Event;
                    break;
                case LexicographerFileName.NounFeeling:
                    context = SemanticContext.Feeling;
                    break;
                case LexicographerFileName.NounFood:
                    context = SemanticContext.Food;
                    break;
                case LexicographerFileName.NounGroup:
                    context = SemanticContext.Group;
                    break;
                case LexicographerFileName.NounLocation:
                    context = SemanticContext.Location;
                    break;
                case LexicographerFileName.NounMotive:
                    context = SemanticContext.Motive;
                    break;
                case LexicographerFileName.NounObject:
                    context = SemanticContext.Object;
                    break;
                case LexicographerFileName.NounPerson:
                    context = SemanticContext.Person;
                    break;
                case LexicographerFileName.NounPhenomenon:
                    context = SemanticContext.Phenomenon;
                    break;
                case LexicographerFileName.NounPlant:
                    context = SemanticContext.Plant;
                    break;
                case LexicographerFileName.NounPossession:
                    context = SemanticContext.Possession;
                    break;
                case LexicographerFileName.NounProcess:
                    context = SemanticContext.Process;
                    break;
                case LexicographerFileName.NounQuantity:
                    context = SemanticContext.Quantity;
                    break;
                case LexicographerFileName.NounRelation:
                    context = SemanticContext.Relation;
                    break;
                case LexicographerFileName.NounShape:
                    context = SemanticContext.Shape;
                    break;
                case LexicographerFileName.NounState:
                    context = SemanticContext.State;
                    break;
                case LexicographerFileName.NounSubstance:
                    context = SemanticContext.Substance;
                    break;
                case LexicographerFileName.NounTime:
                    context = SemanticContext.Time;
                    break;
                case LexicographerFileName.VerbBody:
                    context = SemanticContext.Body;
                    break;
                case LexicographerFileName.VerbChange:
                    context = SemanticContext.Change;
                    break;
                case LexicographerFileName.VerbCognition:
                    context = SemanticContext.Cognition;
                    break;
                case LexicographerFileName.VerbCommunication:
                    context = SemanticContext.Communication;
                    break;
                case LexicographerFileName.VerbCompetition:
                    context = SemanticContext.Competition;
                    break;
                case LexicographerFileName.VerbConsumption:
                    context = SemanticContext.Consumption;
                    break;
                case LexicographerFileName.VerbContact:
                    context = SemanticContext.Contact;
                    break;
                case LexicographerFileName.VerbCreation:
                    context = SemanticContext.Creation;
                    break;
                case LexicographerFileName.VerbEmotion:
                    context = SemanticContext.Emotion;
                    break;
                case LexicographerFileName.VerbMotion:
                    context = SemanticContext.Motion;
                    break;
                case LexicographerFileName.VerbPerception:
                    context = SemanticContext.Perception;
                    break;
                case LexicographerFileName.VerbPossession:
                    context = SemanticContext.Possession;
                    break;
                case LexicographerFileName.VerbSocial:
                    context = SemanticContext.Social;
                    break;
                case LexicographerFileName.VerbStative:
                    context = SemanticContext.Stative;
                    break;
                case LexicographerFileName.VerbWeather:
                    context = SemanticContext.Weather;
                    break;
            }

            return context;
        }
    }
}
