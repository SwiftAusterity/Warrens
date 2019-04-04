using NetMud.Communication.Lexical;
using NetMud.Data.Linguistic;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.Linguistic;
using NetMud.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NetMud.Interp
{
    /// <summary>
    /// Engine for interpreting and merging observances
    /// </summary>
    public class LexicalInterpretationEngine
    {
        private IEnumerable<IDictata> _currentDictionary => ConfigDataCache.GetAll<IDictata>();
        private ILocation _currentPlace;
        private IEntity _actor;

        /// <summary>
        /// Initial experience, takes in an observance and emits the new contexts related to it. Merge and Convey will merge new contexts into existing ones
        /// </summary>
        /// <param name="actor">Who did the action</param>
        /// <param name="action">The raw input that was observed</param>
        /// <returns>A list of the new contexts generated</returns>
        public IEnumerable<IDictata> Parse(IEntity actor, string action, bool push = false)
        {
            List<IDictata> returnList = new List<IDictata>();
            _currentPlace = actor.CurrentLocation.CurrentLocation();
            _actor = actor;

            string[] spaceSplit = action.Split(new char[] { ' ', ',', ':' }, StringSplitOptions.RemoveEmptyEntries);

            Dictionary<string, IDictata> brandedWords = BrandWords(spaceSplit);

            List<string> sentences = new List<string>();

            if (brandedWords.Count(bWord => bWord.Value != null && bWord.Value.WordType == LexicalType.Verb) > 1)
            {
                //multiple verbs means multiple sentences
                IEnumerable<string> punctuationSentences = IsolateSentences(action);

                foreach (string sentence in punctuationSentences)
                {
                    string[] innerSplit = sentence.Split(new char[] { ' ', ',', ':' }, StringSplitOptions.RemoveEmptyEntries);

                    Dictionary<string, IDictata> innerBrands = BrandWords(innerSplit);

                    if (!innerBrands.Any(inWord => inWord.Value != null && inWord.Value.WordType == LexicalType.Conjunction))
                    {
                        sentences.Add(sentence);
                        continue;
                    }

                    bool found = false;
                    foreach ((KeyValuePair<string, IDictata> value, int i) innerVerb in innerBrands.Select((value, i) => (value, i)))
                    {
                        KeyValuePair<string, IDictata> value = innerVerb.value;
                        int index = innerVerb.i;

                        if(value.Value != null && value.Value.WordType != LexicalType.Verb)
                        {
                            continue;
                        }

                        found = true;
                        if (index > 0 && index < innerBrands.Count() - 1)
                        {
                            IDictata wordBefore = innerBrands.ElementAt(index - 1).Value;

                            if(wordBefore.WordType == LexicalType.Conjunction)
                            {
                                int splitIndex = innerBrands.Take(index - 1).Sum(brand => brand.Key.Length + 1);

                                int bumpCount = sentence.Substring(0, splitIndex - 1).Count(ch => ch == ',' || ch == ':');

                                string sentenceOne = sentence.Substring(0, splitIndex - 1 + bumpCount);
                                string sentenceTwo = sentence.Substring(splitIndex + wordBefore.Name.Length + bumpCount);

                                sentences.Add(sentenceOne);
                                sentences.Add(sentenceTwo);
                            }
                        }
                    }

                    if(!found)
                    {
                        sentences.Add(sentence);
                    }
                }
            }
            else
            {
                sentences.Add(action);
            }

            IDictata currentSubject = null;
            foreach (string sentence in sentences)
            {
                IList<Tuple<string, bool>> words = IsolateIndividuals(sentence);

                //can't parse nothing
                if (words.Count == 0)
                {
                    continue;
                }

                IEnumerable<IDictata> fullCommand = ParseAction(words, push, currentSubject);
                currentSubject = fullCommand.LastOrDefault(word => word.WordType == LexicalType.Noun || word.WordType == LexicalType.ProperNoun); 

                returnList.AddRange(fullCommand);
            }

            return returnList;
        }

        /*
         * TODO: Wow this is inefficient, maybe clean up how many loops we do
         */
        private IEnumerable<IDictata> ParseAction(IList<Tuple<string, bool>> words, bool push, IDictata lastSubject)
        {
            /*
             * I kick the can 
             * kick the can
             * kicks the can
             * kick the red can
             * kick the large red can
             */
            List<IDictata> returnList = new List<IDictata>();

            Dictionary<string, IDictata> brandedWords = BrandWords(words);

            IDictata currentVerb = null;

            //Find unknown nouns potentially with conjunctions
            foreach ((KeyValuePair<string, IDictata> value, int i) item in brandedWords.Where(ctx => ctx.Value == null).Select((value, i) => (value, i)).OrderByDescending(keypair => keypair.i))
            {
                KeyValuePair<string, IDictata> value = item.value;
                int index = item.i;

                if (index < brandedWords.Count() - 1 && index > 0)
                {
                    IDictata wordAfter = brandedWords.ElementAt(index + 1).Value;
                    IDictata wordBefore = brandedWords.ElementAt(index - 1).Value;

                    if (wordBefore != null && wordBefore.WordType == LexicalType.Adverb && wordAfter != null && wordAfter.WordType == LexicalType.Verb)
                    {
                        brandedWords[value.Key] = new Dictata() { Name = value.Key, WordType = LexicalType.Adverb };
                        continue;
                    }

                    if ((wordBefore != null && (wordBefore.WordType == LexicalType.Adjective) || wordBefore.WordType == LexicalType.Article)
                        && (wordAfter != null && (wordAfter.WordType == LexicalType.Noun) || wordAfter.WordType == LexicalType.ProperNoun))
                    {
                        brandedWords[value.Key] = new Dictata() { Name = value.Key, WordType = LexicalType.Adjective };
                        continue;
                    }

                    continue;
                }

                if (index < brandedWords.Count() - 1)
                {
                    IDictata wordAfter = brandedWords.ElementAt(index + 1).Value;

                    if (wordAfter != null && wordAfter.WordType == LexicalType.Noun)
                    {
                        brandedWords[value.Key] = new Dictata() { Name = value.Key, WordType = LexicalType.Adjective };
                        continue;
                    }

                    if (wordAfter != null && wordAfter.WordType == LexicalType.Verb)
                    {
                        brandedWords[value.Key] = new Dictata() { Name = value.Key, WordType = LexicalType.Adverb };
                        continue;
                    }
                }

                if (index > 0)
                {
                    IDictata wordBefore = brandedWords.ElementAt(index - 1).Value;

                    if (wordBefore != null && (wordBefore.WordType == LexicalType.Article || wordBefore.WordType == LexicalType.Adjective))
                    {
                        brandedWords[value.Key] = new Dictata() { Name = value.Key, WordType = LexicalType.Noun };
                        continue;
                    }

                    if (wordBefore != null && wordBefore.WordType == LexicalType.Adverb)
                    {
                        brandedWords[value.Key] = new Dictata() { Name = value.Key, WordType = LexicalType.Verb };
                        continue;
                    }
                }
            }

            //No verb?
            if (!brandedWords.Any(ctx => ctx.Value != null && ctx.Value.WordType == LexicalType.Verb))
            {
                string verbWord = brandedWords.First(ctx => ctx.Value == null).Key;

                currentVerb = new Dictata() { Name = verbWord, WordType = LexicalType.Verb  };
                brandedWords[verbWord] = currentVerb;
            }
            else
            {
                currentVerb = brandedWords.FirstOrDefault(ctx => ctx.Value != null && ctx.Value.WordType == LexicalType.Verb).Value;
            }

            //We might have nouns already
            if (!brandedWords.Any(ctx => ctx.Value == null || (ctx.Value != null && 
                (ctx.Value.WordType == LexicalType.Noun || ctx.Value.WordType == LexicalType.ProperNoun))))
            {
                bool lastSubjectReplaced = false;
                if (lastSubject != null)
                {
                    List<string> keyList = new List<string>();
                    foreach(KeyValuePair<string, IDictata> word in brandedWords.Where(ctx => ctx.Value != null && ctx.Value.WordType == LexicalType.Pronoun))
                    {
                        keyList.Add(word.Key);
                        lastSubjectReplaced = true;
                    }

                    foreach(string key in keyList)
                    {
                        brandedWords[key] = (IDictata)lastSubject.Clone();
                    }
                }

                if (!lastSubjectReplaced)
                {
                    string targetWord = string.Empty;

                    //No valid nouns to make the target? Pick the last one
                    if (!brandedWords.Any(ctx => ctx.Value == null))
                    {
                        targetWord = brandedWords.LastOrDefault().Key;
                    }
                    else
                    {
                        targetWord = brandedWords.LastOrDefault(ctx => ctx.Value == null).Key;
                    }

                    brandedWords[targetWord] = new Dictata() { Name = targetWord, WordType = LexicalType.Noun };
                }
            }

            List<IDictata> descriptors = new List<IDictata>();
            foreach ((KeyValuePair<string, IDictata> value, int i) item in brandedWords.Where(ctx => ctx.Value == null).Select((value, i) => (value, i)))
            {
                KeyValuePair<string, IDictata> value = item.value;
                int index = item.i;

                LexicalType wordType = LexicalType.Adjective;
                if (index == brandedWords.Count() - 1)
                {
                    IDictata wordAfter = brandedWords.ElementAt(index + 1).Value;

                    if (wordAfter != null)
                    {
                        if (wordAfter.WordType == LexicalType.Verb)
                        {
                            wordType = LexicalType.Adverb;
                        }

                        if (wordAfter.WordType == LexicalType.Pronoun)
                        {
                            wordType = LexicalType.Article;
                        }
                    }
                }

                descriptors.Add(new Dictata() { Name = value.Key, WordType = wordType });
            }

            //Add the nonadjectives and the adjectives
            returnList.AddRange(brandedWords.Where(bws => bws.Value != null).Select(bws => bws.Value));
            returnList.AddRange(descriptors.Select(desc => desc));

            if (push)
            {
                foreach (IDictata item in returnList)
                {
                    LexicalProcessor.VerifyLexeme(item.GetLexeme());
                }
            }

            return returnList;
        }

        /*
         * TODO: First pass: parse out existing things, the place we're in and decorators
         * Second pass: search for places in the world to make links
         * Third pass: More robust logic to avoid extra merging later
         */
        private IEnumerable<IDictata> ParseSpeech(IList<Tuple<string, bool>> words)
        {
            /*
             * hello
             * hi there
             * did you go to the store
             * what are you doing there
             * I saw a red ball in the living room
             */
            List<IDictata> returnList = new List<IDictata>();

            ILocation currentPlace = _actor.CurrentLocation.CurrentLocation();

            Dictionary<string, IDictata> brandedWords = BrandWords(words);

            string targetWord = string.Empty;

            //No valid nouns to make the target? Pick the last one
            if (!brandedWords.Any(ctx => ctx.Value == null))
            {
                targetWord = brandedWords.LastOrDefault().Key;
            }
            else
            {
                targetWord = brandedWords.LastOrDefault(ctx => ctx.Value == null).Key;
            }

            brandedWords.Remove(targetWord);

            List<IDictata> descriptors = new List<IDictata>();
            foreach (KeyValuePair<string, IDictata> adjective in brandedWords.Where(ctx => ctx.Value == null 
                                                                                        || (ctx.Value != null && (ctx.Value.WordType == LexicalType.Adjective
                                                                                                                || ctx.Value.WordType == LexicalType.Adverb))))
            {
                if (adjective.Value != null)
                {
                    descriptors.Add(adjective.Value);
                }
                else
                {
                    descriptors.Add(new Dictata() { Name = adjective.Key, WordType = LexicalType.Adjective });
                }
            }

            returnList.AddRange(descriptors);

            return returnList;
        }

        private Dictionary<string, IDictata> BrandWords(string [] words)
        {
            List<Tuple<string, bool>> blankSlate = new List<Tuple<string, bool>>();

            blankSlate.AddRange(words.Select(word => new Tuple<string, bool>(word, false)));

            return BrandWords(blankSlate);
        }

        private Dictionary<string, IDictata> BrandWords(IList<Tuple<string, bool>> words)
        {
            Dictionary<string, IDictata> brandedWords = new Dictionary<string, IDictata>();

            //Brand all the words with their current meaning. Continues are in there because the listword inflation might cause collision
            foreach (Tuple<string, bool> word in words.Distinct())
            {
                if (brandedWords.ContainsKey(word.Item1))
                {
                    continue;
                }

                //We have a comma/and list
                if (word.Item2)
                {
                    string[] listWords = word.Item1.Split(new string[] { "and", ",", " " }, StringSplitOptions.RemoveEmptyEntries);

                    IDictata listMeaning = null;
                    foreach (string listWord in listWords)
                    {
                        if (listMeaning != null)
                        {
                            break;
                        }

                        if (brandedWords.ContainsKey(listWord))
                        {
                            listMeaning = brandedWords[listWord];
                        }

                        if (listMeaning == null)
                        {
                            listMeaning = GetExistingMeaning(listWord);
                        }
                    }

                    foreach (string listWord in listWords)
                    {
                        if (brandedWords.ContainsKey(listWord))
                        {
                            continue;
                        }

                        Dictata meaning = new Dictata()
                        {
                            Name = listWord,
                            WordType = listMeaning.WordType
                        };

                        brandedWords.Add(listWord, meaning);
                    }

                    continue;
                }

                brandedWords.Add(word.Item1, GetExistingMeaning(word.Item1));
            }

            return brandedWords;
        }

        private IDictata GetExistingMeaning(string word)
        {
            List<string> allContext = new List<string>();

            //Get all local nouns
            allContext.AddRange(_currentPlace.GetContents<IInanimate>().SelectMany(thing => thing.Keywords));
            allContext.AddRange(_currentPlace.GetContents<IMobile>().SelectMany(thing => thing.Keywords));
            allContext.AddRange(_currentPlace.Keywords);
            allContext.AddRange(_actor.Keywords);

            IDictata existingMeaning = null;

            //It's a thing we can see
            if (allContext.Contains(word))
            {
                existingMeaning = new Dictata() { Name = word, WordType = LexicalType.ProperNoun };
            }
            else
            {
                //TODO: We need to discriminate based on lexical type as well, we could have multiple of the same word with different types
                if (_currentDictionary.Any(dict => dict.Name.Equals(word, StringComparison.InvariantCultureIgnoreCase)))
                {
                    existingMeaning = _currentDictionary.FirstOrDefault(dict => dict.Name.Equals(word, StringComparison.InvariantCultureIgnoreCase));
                }
            }

            return existingMeaning;
        }

        private IList<Tuple<string, bool>> IsolateIndividuals(string baseString)
        {
            int iterator = 0;
            baseString = baseString.ToLower();

            List<Tuple<string, bool>> foundStrings = ParseQuotesOut(ref baseString, ref iterator);

            foundStrings.AddRange(ParseEntitiesOut(ref iterator, ref baseString));

            foundStrings.AddRange(ParseCommaListsOut(ref iterator, ref baseString));

            List<string> originalStrings = new List<string>();
            originalStrings.AddRange(baseString.Split(new char[] { ' ', ',', ':' }, StringSplitOptions.RemoveEmptyEntries));

            //So thanks to the commalist puller potentially adding replacement strings to the found collection we have to do a pass there first
            List<Tuple<int, string>> cleanerList = new List<Tuple<int, string>>();
            foreach (Tuple<string, bool> dirtyString in foundStrings.Where(str => str.Item1.Contains("%%")))
            {
                int dirtyIndex = foundStrings.IndexOf(dirtyString);
                string cleanString = dirtyString.Item1;

                while (cleanString.Contains("%%"))
                {
                    int i = DataUtility.TryConvert<int>(cleanString.Substring(cleanString.IndexOf("%%") + 2, 1));
                    cleanString = cleanString.Replace(string.Format("%%{0}%%", i), foundStrings[i].Item1);
                }

                cleanerList.Add(new Tuple<int, string>(dirtyIndex, cleanString));
            }

            foreach (Tuple<int, string> cleaner in cleanerList)
            {
                int dirtyIndex = cleaner.Item1;
                string cleanString = cleaner.Item2;

                foundStrings[dirtyIndex] = new Tuple<string, bool>(cleanString, foundStrings[dirtyIndex].Item2);
            }

            //Either add the modified one or add the normal one
            List<Tuple<string, bool>> returnStrings = new List<Tuple<string, bool>>();
            foreach (string returnString in originalStrings)
            {
                if (returnString.StartsWith("%%") && returnString.EndsWith("%%"))
                {
                    int i = DataUtility.TryConvert<int>(returnString.Substring(2, returnString.Length - 4));
                    returnStrings.Add(foundStrings[i]);
                }
                else
                {
                    returnStrings.Add(new Tuple<string, bool>(returnString, false));
                }
            }

            return returnStrings;
        }

        /*
         * word, word, word, word- ([a-zA-Z0-9_.-|(%%\d%%)]+)((,|,\s)[a-zA-Z0-9_.-|(%%\d%%)]+)+
         * word, word, word and word- ([a-zA-Z0-9_.-|(%%\d%%)]+)((,|,\s)[a-zA-Z0-9_.-|(%%\d%%)]+)+(\sand\s)([a-zA-Z0-9_.-|(%%\d%%)]+)
         * word, word, word, and word ([a-zA-Z0-9_.-|(%%\d%%)]+)((,|,\s)[a-zA-Z0-9_.-|(%%\d%%)]+)+(,\sand\s)([a-zA-Z0-9_.-|(%%\d%%)]+)
         * word and word and word and word- ([a-zA-Z0-9_.-|(%%\d%%)]+)((\sand\s)[a-zA-Z0-9_.-|(%%\d%%)]+)+
         */
        private IList<Tuple<string, bool>> ParseCommaListsOut(ref int iterator, ref string baseString)
        {
            List<Tuple<string, bool>> foundStrings = new List<Tuple<string, bool>>();
            Regex cccPattern = new Regex(@"([a-zA-Z0-9_.-|(%%\d%%)]+)((,|,\s)[a-zA-Z0-9_.-|(%%\d%%)]+)+", RegexOptions.IgnorePatternWhitespace);
            Regex ccaPattern = new Regex(@"([a-zA-Z0-9_.-|(%%\d%%)]+)((,|,\s)[a-zA-Z0-9_.-|(%%\d%%)]+)+(\sand\s)([a-zA-Z0-9_.-|(%%\d%%)]+)", RegexOptions.IgnorePatternWhitespace);
            Regex ccacPattern = new Regex(@"([a-zA-Z0-9_.-|(%%\d%%)]+)((,|,\s)[a-zA-Z0-9_.-|(%%\d%%)]+)+(,\sand\s)([a-zA-Z0-9_.-|(%%\d%%)]+)", RegexOptions.IgnorePatternWhitespace);
            Regex aaaPattern = new Regex(@"([a-zA-Z0-9_.-|(%%\d%%)]+)((\sand\s)[a-zA-Z0-9_.-|(%%\d%%)]+)+", RegexOptions.IgnorePatternWhitespace);

            foundStrings.AddRange(RunListPattern(ccacPattern, ref iterator, ref baseString));
            foundStrings.AddRange(RunListPattern(ccaPattern, ref iterator, ref baseString));
            foundStrings.AddRange(RunListPattern(aaaPattern, ref iterator, ref baseString));
            foundStrings.AddRange(RunListPattern(cccPattern, ref iterator, ref baseString));

            return foundStrings;
        }

        private IList<Tuple<string, bool>> RunListPattern(Regex capturePattern, ref int iterator, ref string baseString)
        {
            List<Tuple<string, bool>> foundStrings = new List<Tuple<string, bool>>();

            MatchCollection cccMatches = capturePattern.Matches(baseString);
            for (int i = 0; i < cccMatches.Count; i++)
            {
                Match currentMatch = cccMatches[i];

                if (currentMatch == null || !currentMatch.Success)
                {
                    continue;
                }

                CaptureCollection cccCaptures = currentMatch.Captures;
                for (int iC = 0; iC < cccCaptures.Count; iC++)
                {
                    Capture currentCapture = cccCaptures[iC];

                    if (currentCapture == null || currentCapture.Length == 0)
                    {
                        continue;
                    }

                    string commaList = currentCapture.Value;

                    foundStrings.Add(new Tuple<string, bool>(commaList, true));
                    baseString = baseString.Replace(commaList, "%%" + iterator.ToString() + "%%");
                    iterator++;
                }
            }

            return foundStrings;
        }

        private IList<Tuple<string, bool>> ParseEntitiesOut(ref int iterator, ref string baseString)
        {
            List<Tuple<string, bool>> foundStrings = new List<Tuple<string, bool>>();
            List<string> allContext = new List<string>();

            //Get all local nouns
            allContext.AddRange(_actor.CurrentLocation.CurrentLocation().GetContents<IInanimate>().SelectMany(thing => thing.Keywords));
            allContext.AddRange(_actor.CurrentLocation.CurrentLocation().GetContents<IMobile>().SelectMany(thing => thing.Keywords));
            allContext.AddRange(_actor.CurrentLocation.CurrentLocation().Keywords);
            allContext.AddRange(_actor.Keywords);

            //Brand all the words with their current meaning
            foreach (string word in allContext.Distinct())
            {
                if (baseString.Contains(word))
                {
                    foundStrings.Add(new Tuple<string, bool>(word, false));
                    baseString = baseString.Replace(word, "%%" + iterator.ToString() + "%%");
                    iterator++;
                }
            }

            return foundStrings;
        }

        /// <summary>
        /// Scrubs "s and 's out and figures out what the parameters really are
        /// </summary>
        /// <returns>the right parameters</returns>
        private List<Tuple<string, bool>> ParseQuotesOut(ref string baseString, ref int iterator)
        {
            List<Tuple<string, bool>> foundStrings = new List<Tuple<string, bool>>();

            baseString = IsolateStrings(baseString, "\"", foundStrings, ref iterator);
            baseString = IsolateStrings(baseString, "'", foundStrings, ref iterator);

            return foundStrings;
        }

        //Do we have magic string collectors? quotation marks demarcate a single parameter being passed in
        private string IsolateStrings(string baseString, string closure, List<Tuple<string, bool>> foundStrings, ref int foundStringIterator)
        {
            while (baseString.Contains("\""))
            {
                int firstQuoteIndex = baseString.IndexOf('"');
                int secondQuoteIndex = baseString.IndexOf('"', firstQuoteIndex + 1);

                //What? Why would this even happen
                if (firstQuoteIndex < 0)
                {
                    break;
                }

                //Only one means let's just kill the stupid quotemark and move on
                if (secondQuoteIndex < 0)
                {
                    baseString = baseString.Replace("\"", string.Empty);
                    break;
                }

                string foundString = baseString.Substring(firstQuoteIndex + 1, secondQuoteIndex - firstQuoteIndex - 1);

                foundStrings.Add(new Tuple<string, bool>(foundString, false));
                baseString = baseString.Replace(string.Format("\"{0}\"", foundString), "%%" + foundStringIterator.ToString() + "%%");
                foundStringIterator++;
            }

            return baseString;
        }

        IEnumerable<string> IsolateSentences(string input)
        {
            List<string> sentences = new List<string>();

            //TODO: recognize "and <verb>"
            string[] initialSplit = input.Split(new string[] { ";", "?", ". ", "!" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string phrase in initialSplit)
            {
                string[] potentialWords = phrase.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (potentialWords.Count() > 1)
                {
                    sentences.Add(phrase);
                }
            }

            if (sentences.Count > 1)
            {
                return sentences;
            }

            //Fall back to just the initial sentence because we couldn't find multiple full sentences.
            return new List<string>() { input };
        }

        /// <summary>
        /// Removes stuff we don't care about like to, into, the, etc
        /// </summary>
        /// <param name="currentParams">The current set of params</param>
        /// <returns>the scrubbed params</returns>
        private IList<string> RemoveGrammaticalNiceities(IList<string> currentParams)
        {
            List<string> parmList = currentParams.ToList();

            parmList.RemoveAll(str => str.Equals("the", StringComparison.InvariantCulture)
                                        || str.Equals("of", StringComparison.InvariantCulture)
                                        || str.Equals("to", StringComparison.InvariantCulture)
                                        || str.Equals("into", StringComparison.InvariantCulture)
                                        || str.Equals("in", StringComparison.InvariantCulture)
                                        || str.Equals("from", StringComparison.InvariantCulture)
                                        || str.Equals("inside", StringComparison.InvariantCulture)
                                        || str.Equals("at", StringComparison.InvariantCulture)
                                        || str.Equals("a", StringComparison.InvariantCulture)
                                        || str.Equals("an", StringComparison.InvariantCulture)
                                        || str.Equals("that", StringComparison.InvariantCulture)
                                        || str.Equals("this", StringComparison.InvariantCulture)
                                  );

            return parmList;
        }
    }
}
