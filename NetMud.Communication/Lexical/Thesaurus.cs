using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.System;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace NetMud.Communication.Lexical
{
    /// <summary>
    /// Finds synonyms and antonyms
    /// </summary>
    public static class Thesaurus
    {
        public static string GetTranslatedWord(string azureKey, string phrase, ILanguage sourceLanguage, ILanguage targetLanguage)
        {
            if (string.IsNullOrWhiteSpace(targetLanguage.GoogleLanguageCode) || string.IsNullOrWhiteSpace(sourceLanguage.GoogleLanguageCode))
            {
                return string.Empty;
            }

            string host = "https://api.cognitive.microsofttranslator.com";
            string route = string.Format("/translate?api-version=3.0&to={0}", targetLanguage.GoogleLanguageCode);
            string subscriptionKey = azureKey;

            try
            {
                object[] body = new object[] { new { Text = phrase } };
                string requestBody = JsonConvert.SerializeObject(body);

                using (HttpClient client = new())
                using (HttpRequestMessage request = new())
                {
                    // Set the method to POST
                    request.Method = HttpMethod.Post;

                    // Construct the full URI
                    request.RequestUri = new Uri(host + route);

                    // Add the serialized JSON object to your request
                    request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                    // Add the authorization header
                    request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

                    // Send request, get response
                    HttpResponseMessage response = client.SendAsync(request).Result;
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    dynamic result = JsonConvert.DeserializeObject(jsonResponse);

                    return result[0].translations[0].text;

                    //[
                    //  {
                    //    "detectedLanguage": {
                    //      "language": "en",
                    //      "score": 1.0
                    //    },
                    //    "translations": [
                    //      {
                    //        "text": "Hallo Welt!",
                    //        "to": "de"
                    //      },
                    //      {
                    //        "text": "Salve, mondo!",
                    //        "to": "it"
                    //      }
                    //    ]
                    //  }
                    //]
                }
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return string.Empty;
        }

        #region word to word
        public static IDictata ObscureWord(IDictata word, short obscureStrength)
        {
            if (word.Language == null)
            {
                IGlobalConfig globalConfig = ConfigDataCache.Get<IGlobalConfig>(new ConfigDataCacheKey(typeof(IGlobalConfig), "LiveSettings", ConfigDataType.GameWorld));

                word.Language = globalConfig.BaseLanguage;
            }

            IEnumerable<IDictata> possibleWords = ConfigDataCache.GetAll<ILexeme>().Where(dict => dict.SuitableForUse 
                                                                                && dict.Language == word.Language 
                                                                                && dict.GetForm(word.WordType) != null).Select(lex => lex.GetForm(word.WordType));

            return GetObscuredWord(word, possibleWords, obscureStrength);
        }

        public static IDictata GetWord(LexicalContext context, LexicalType type)
        {
            if (context.Language == null)
            {
                IGlobalConfig globalConfig = ConfigDataCache.Get<IGlobalConfig>(new ConfigDataCacheKey(typeof(IGlobalConfig), "LiveSettings", ConfigDataType.GameWorld));

                context.Language = globalConfig.BaseLanguage;
            }

            IEnumerable<IDictata> possibleWords = ConfigDataCache.GetAll<ILexeme>()
                .Where(dict => dict.Language == context.Language && dict.GetForm(type) != null && dict.SuitableForUse).Select(lex => lex.GetForm(type));

            return possibleWords.OrderByDescending(word => GetSynonymRanking(word, context)).FirstOrDefault();
        }

        public static IDictata GetAntonym(IDictata baseWord, LexicalContext context)
        {
            if (baseWord == null)
            {
                return baseWord;
            }

            return FocusFindWord(baseWord.Antonyms.ToList(), context, baseWord);
        }

        public static IDictata GetSynonym(IDictata baseWord, LexicalContext context)
        {
            if (baseWord == null)
            {
                return baseWord;
            }

            return FocusFindWord(baseWord.Synonyms.ToList(), context, baseWord);
        }

        private static IDictata FocusFindWord(IEnumerable<IDictata> possibleWords, LexicalContext context, IDictata baseWord)
        {
            if (context.Language == null)
            {
                context.Language = baseWord.Language;
            }

            possibleWords = possibleWords.Where(word => word != null && word.Language == context.Language && word.GetLexeme().SuitableForUse);

            if (context.Severity + context.Elegance + context.Quality == 0)
            {
                List<Tuple<IDictata, int>> rankedWords = new()
                {
                    new Tuple<IDictata, int>(baseWord, GetSynonymRanking(baseWord, context))
                };

                rankedWords.AddRange(possibleWords.Select(word => new Tuple<IDictata, int>(word, GetSynonymRanking(word, context))));

                return rankedWords.OrderByDescending(pair => pair.Item2).Select(pair => pair.Item1).FirstOrDefault();
            }

            return GetRelatedWord(baseWord, possibleWords, context.Severity, context.Elegance, context.Quality);
        }

        private static int GetSynonymRanking(IDictata word, LexicalContext context)
        {
            return (word.Positional == context.Position ? 5 : 0) +
                    (word.Tense == context.Tense ? 5 : 0) +
                    (word.Perspective == context.Perspective ? 5 : 0) +
                    (word.Possessive == context.Possessive ? 7 : 0) +
                    ((context.GenderForm == null || word.Feminine == context.GenderForm?.Feminine) ? 10 : 0) +
                    (word.Plural == context.Plural ? 2 : 0) +
                    (word.Determinant == context.Determinant ? 2 : 0) +
                    (context.Semantics.Any() ? word.Semantics.Count(wrd => context.Semantics.Contains(wrd)) * 10 : 0);
        }

        private static IDictata GetRelatedWord(IDictata baseWord, IEnumerable<IDictata> possibleWords, int severityModifier, int eleganceModifier, int qualityModifier)
        {
            Dictionary<IDictata, int> rankedWords = new();
            foreach (IDictata word in possibleWords)
            {
                int rating = 0;

                rating += Math.Abs(baseWord.Severity + severityModifier - word.Severity);
                rating += Math.Abs(baseWord.Elegance + eleganceModifier - word.Elegance);
                rating += Math.Abs(baseWord.Quality + qualityModifier - word.Quality);

                rankedWords.Add(word, rating);
            }

            KeyValuePair<IDictata, int> closestWord = rankedWords.OrderBy(pair => pair.Value).FirstOrDefault();

            return closestWord.Key ?? baseWord;
        }

        private static IDictata GetObscuredWord(IDictata word, IEnumerable<IDictata> possibleWords, short obscureStrength)
        {
            if (word == null || possibleWords.Count() == 0 || obscureStrength == 0)
            {
                return word;
            }

            //try to downgrade word
            Dictionary<IDictata, int> rankedWords = new();
            foreach (IDictata possibleWord in possibleWords)
            {
                int rating = Math.Abs(word.Quality + (Math.Abs(obscureStrength) * -1) - possibleWord.Quality);

                rankedWords.Add(possibleWord, rating);
            }

            KeyValuePair<IDictata, int> closestWord = rankedWords.OrderBy(pair => pair.Value).FirstOrDefault();
            IDictata newWord = closestWord.Key;

            LexicalType[] descriptiveWordTypes = new LexicalType[] { LexicalType.Adjective, LexicalType.Adverb };
            LexicalType[] remainderWordTypes = new LexicalType[] { LexicalType.Verb, LexicalType.Preposition, LexicalType.Conjunction, LexicalType.Article };
            LexicalType[] nounWordTypes = new LexicalType[] { LexicalType.Pronoun, LexicalType.ProperNoun, LexicalType.Noun };
            if (newWord != null)
            {
                //Adjectives/adverbs/articles get eaten
                if (descriptiveWordTypes.Contains(newWord.WordType))
                {
                    newWord = null;
                }

                //if it's a verb or preposition or structural leave it alone
                if (remainderWordTypes.Contains(newWord.WordType))
                {
                    newWord = word;
                }

                //pronouns become "it"
                if (nounWordTypes.Contains(newWord.WordType))
                {
                    LexicalContext itContext = new(null)
                    {
                        Determinant = false,
                        Plural = false,
                        Possessive = false,
                        Tense = LexicalTense.None,
                        Language = word.Language,
                        Perspective = NarrativePerspective.None
                    };

                    newWord = GetWord(itContext, LexicalType.Pronoun);
                }

                //TODO: if it's a noun try to downgrade it to a shape or single aspect
            }
            else
            {
                newWord = word;
            }

            return newWord;
        }
        #endregion
    }
}
