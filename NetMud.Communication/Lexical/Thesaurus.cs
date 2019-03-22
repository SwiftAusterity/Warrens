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
                var requestBody = JsonConvert.SerializeObject(body);

                using (var client = new HttpClient())
                using (var request = new HttpRequestMessage())
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
                    var response = client.SendAsync(request).Result;
                    var jsonResponse = response.Content.ReadAsStringAsync().Result;

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
        public static IDictata GetWord(LexicalContext context, LexicalType type)
        {
            if (context.Language == null)
            {
                IGlobalConfig globalConfig = ConfigDataCache.Get<IGlobalConfig>(new ConfigDataCacheKey(typeof(IGlobalConfig), "LiveSettings", ConfigDataType.GameWorld));

                context.Language = globalConfig.BaseLanguage;
            }

            var possibleWords = ConfigDataCache.GetAll<IDictata>().Where(dict => dict.Language == context.Language && dict.WordTypes.Contains(type) && dict.SuitableForUse);

            return possibleWords.OrderByDescending(word => GetSynonymRanking(word, context)).FirstOrDefault();
        }

        public static IDictata GetAntonym(IDictata baseWord, LexicalContext context)
        {
            if (baseWord == null)
                return baseWord;

            return FocusFindWord(baseWord.Antonyms.ToList(), context, baseWord);
        }

        public static IDictata GetSynonym(IDictata baseWord, LexicalContext context)
        {
            if (baseWord == null)
                return baseWord;

            return FocusFindWord(baseWord.Synonyms.ToList(), context, baseWord);
        }

        private static IDictata FocusFindWord(IEnumerable<IDictata> possibleWords, LexicalContext context, IDictata baseWord)
        {
            if (context.Language == null)
            {
                context.Language = baseWord.Language;
            }

            possibleWords = possibleWords.Where(word => word != null && word.Language == context.Language && word.SuitableForUse);

            if (context.Severity + context.Elegance + context.Quality == 0)
            {
                var rankedWords = new List<Tuple<IDictata, int>>
                {
                    new Tuple<IDictata, int>(baseWord, GetSynonymRanking(baseWord, context))
                };

                rankedWords.AddRange(possibleWords.Select(word => new Tuple<IDictata, int>(word, GetSynonymRanking(word, context))));

                return rankedWords.OrderByDescending(pair => pair.Item2).Select(pair => pair.Item1).FirstOrDefault();
            }

            return GetRelatedWord(baseWord, possibleWords, context.Severity, context.Elegance, context.Quality);
        }

        private static IDictata FocusFindWord(IEnumerable<IDictata> possibleWords, LexicalContext context, IDictataPhrase basePhrase)
        {
            if (context.Language == null)
            {
                context.Language = basePhrase.Language;
            }

            possibleWords = possibleWords.Where(word => word != null && word.Language == context.Language && word.SuitableForUse);

            if (context.Severity + context.Elegance + context.Quality == 0)
            {
                var rankedWords = new List<Tuple<IDictata, int>>();
                var baseRanking = GetSynonymRanking(basePhrase, context);

                rankedWords.AddRange(possibleWords.Select(word => new Tuple<IDictata, int>(word, GetSynonymRanking(word, context))));

                if (baseRanking > rankedWords.Max(phrase => phrase.Item2))
                {
                    return null;
                }

                return rankedWords.OrderByDescending(pair => pair.Item2).Select(pair => pair.Item1).FirstOrDefault();
            }

            return GetRelatedWord(basePhrase, possibleWords, context.Severity, context.Elegance, context.Quality);
        }

        private static int GetSynonymRanking(IDictata word, LexicalContext context)
        {
            return  (word.Positional == context.Position ? 5 : 0) +
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
            var rankedWords = new Dictionary<IDictata, int>();
            foreach (var word in possibleWords)
            {
                int rating = 0;

                rating += Math.Abs(baseWord.Severity + severityModifier - word.Severity);
                rating += Math.Abs(baseWord.Elegance + eleganceModifier - word.Elegance);
                rating += Math.Abs(baseWord.Quality + qualityModifier - word.Quality);

                rankedWords.Add(word, rating);
            }

            var closestWord = rankedWords.OrderBy(pair => pair.Value).FirstOrDefault();

            return closestWord.Key ?? baseWord;
        }

        private static IDictata GetRelatedWord(IDictataPhrase basePhrase, IEnumerable<IDictata> possibleWords, int severityModifier, int eleganceModifier, int qualityModifier)
        {
            var rankedWords = new Dictionary<IDictata, int>();
            foreach (var word in possibleWords)
            {
                int rating = 0;

                rating += Math.Abs(basePhrase.Severity + severityModifier - word.Severity);
                rating += Math.Abs(basePhrase.Elegance + eleganceModifier - word.Elegance);
                rating += Math.Abs(basePhrase.Quality + qualityModifier - word.Quality);

                rankedWords.Add(word, rating);
            }

            var closestWord = rankedWords.OrderBy(pair => pair.Value).FirstOrDefault();

            return closestWord.Key;
        }
        #endregion

        #region Phrases
        public static IDictataPhrase GetPhrase(LexicalContext context)
        {
            if (context.Language == null)
            {
                IGlobalConfig globalConfig = ConfigDataCache.Get<IGlobalConfig>(new ConfigDataCacheKey(typeof(IGlobalConfig), "LiveSettings", ConfigDataType.GameWorld));

                context.Language = globalConfig.BaseLanguage;
            }

            var possibleWords = ConfigDataCache.GetAll<IDictataPhrase>().Where(dict => dict.Language == context.Language && dict.SuitableForUse);

            return possibleWords.OrderByDescending(word => GetSynonymRanking(word, context)).FirstOrDefault();
        }

        public static IDictata GetAntonym(IDictataPhrase basePhrase, LexicalContext context)
        {
            if (basePhrase == null)
                return null;

            return FocusFindWord(basePhrase.Antonyms.ToList(), context, basePhrase);
        }

        public static IDictata GetSynonym(IDictataPhrase basePhrase, LexicalContext context)
        {
            if (basePhrase == null)
                return null;

            return FocusFindWord(basePhrase.Synonyms.ToList(), context, basePhrase);
        }

        public static IDictataPhrase GetAntonymPhrase(IDictata baseWord, LexicalContext context)
        {
            if (baseWord == null)
                return null;

            return FocusFindPhrase(baseWord.PhraseAntonyms.ToList(), context, baseWord);
        }

        public static IDictataPhrase GetSynonymPhrase(IDictata baseWord, LexicalContext context)
        {
            if (baseWord == null)
                return null;

            return FocusFindPhrase(baseWord.PhraseSynonyms.ToList(), context, baseWord);
        }

        public static IDictataPhrase GetAntonymPhrase(IDictataPhrase basePhrase, LexicalContext context)
        {
            if (basePhrase == null)
                return basePhrase;

            return FocusFindPhrase(basePhrase.PhraseAntonyms.ToList(), context, basePhrase);
        }

        public static IDictataPhrase GetSynonymPhrase(IDictataPhrase basePhrase, LexicalContext context)
        {
            if (basePhrase == null)
                return basePhrase;

            return FocusFindPhrase(basePhrase.PhraseSynonyms.ToList(), context, basePhrase);
        }

        private static IDictataPhrase FocusFindPhrase(IEnumerable<IDictataPhrase> possiblePhrases, LexicalContext context, IDictata baseWord)
        {
            if (context.Language == null)
            {
                context.Language = baseWord.Language;
            }

            possiblePhrases = possiblePhrases.Where(phrase => phrase != null && phrase.Language == context.Language && phrase.SuitableForUse);

            if (context.Severity + context.Elegance + context.Quality == 0)
            {
                var rankedPhrases = new List<Tuple<IDictataPhrase, int>>();
                var baseRanking = GetSynonymRanking(baseWord, context);

                rankedPhrases.AddRange(possiblePhrases.Select(phrase => new Tuple<IDictataPhrase, int>(phrase, GetSynonymRanking(phrase, context))));

                if(baseRanking > rankedPhrases.Max(phrase => phrase.Item2))
                {
                    return null;
                }

                return rankedPhrases.OrderByDescending(pair => pair.Item2).Select(pair => pair.Item1).FirstOrDefault();
            }

            return GetRelatedPhrase(baseWord, possiblePhrases, context.Severity, context.Elegance, context.Quality);
        }

        private static IDictataPhrase FocusFindPhrase(IEnumerable<IDictataPhrase> possiblePhrases, LexicalContext context, IDictataPhrase basePhrase)
        {
            if (context.Language == null)
            {
                context.Language = basePhrase.Language;
            }

            possiblePhrases = possiblePhrases.Where(phrase => phrase != null && phrase.Language == context.Language && phrase.SuitableForUse);

            if (context.Severity + context.Elegance + context.Quality == 0)
            {
                var rankedPhrases = new List<Tuple<IDictataPhrase, int>>
                {
                    new Tuple<IDictataPhrase, int>(basePhrase, GetSynonymRanking(basePhrase, context))
                };

                rankedPhrases.AddRange(possiblePhrases.Select(phrase => new Tuple<IDictataPhrase, int>(phrase, GetSynonymRanking(phrase, context))));

                return rankedPhrases.OrderByDescending(pair => pair.Item2).Select(pair => pair.Item1).FirstOrDefault();
            }

            return GetRelatedPhrase(basePhrase, possiblePhrases, context.Severity, context.Elegance, context.Quality);
        }

        private static int GetSynonymRanking(IDictataPhrase word, LexicalContext context)
        {
            return (word.Positional == context.Position ? 5 : 0) +
                    (word.Tense == context.Tense ? 5 : 0) +
                    (word.Perspective == context.Perspective ? 5 : 0) +
                    ((context.GenderForm == null || word.Feminine == context.GenderForm?.Feminine) ? 10 : 0) +
                    (context.Semantics.Any() ? word.Semantics.Count(wrd => context.Semantics.Contains(wrd)) * 10 : 0);
        }

        private static IDictataPhrase GetRelatedPhrase(IDictata baseWord, IEnumerable<IDictataPhrase> possibleWords, int severityModifier, int eleganceModifier, int qualityModifier)
        {
            var rankedPhrasess = new Dictionary<IDictataPhrase, int>();
            foreach (var word in possibleWords)
            {
                int rating = 0;

                rating += Math.Abs(baseWord.Severity + severityModifier - word.Severity);
                rating += Math.Abs(baseWord.Elegance + eleganceModifier - word.Elegance);
                rating += Math.Abs(baseWord.Quality + qualityModifier - word.Quality);

                rankedPhrasess.Add(word, rating);
            }

            var closestPhrase = rankedPhrasess.OrderBy(pair => pair.Value).FirstOrDefault();

            return closestPhrase.Key;
        }

        private static IDictataPhrase GetRelatedPhrase(IDictataPhrase basePhrase, IEnumerable<IDictataPhrase> possibleWords, int severityModifier, int eleganceModifier, int qualityModifier)
        {
            var rankedPhrasess = new Dictionary<IDictataPhrase, int>();
            foreach (var word in possibleWords)
            {
                int rating = 0;

                rating += Math.Abs(basePhrase.Severity + severityModifier - word.Severity);
                rating += Math.Abs(basePhrase.Elegance + eleganceModifier - word.Elegance);
                rating += Math.Abs(basePhrase.Quality + qualityModifier - word.Quality);

                rankedPhrasess.Add(word, rating);
            }

            var closestPhrase = rankedPhrasess.OrderBy(pair => pair.Value).FirstOrDefault();

            return closestPhrase.Key ?? basePhrase;
        }
        #endregion
    }
}
