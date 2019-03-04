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

            // return string.Empty;

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

        public static IDictata GetWord(LexicalContext context, LexicalType type)
        {
            if (context.Language == null)
            {
                IGlobalConfig globalConfig = ConfigDataCache.Get<IGlobalConfig>(new ConfigDataCacheKey(typeof(IGlobalConfig), "LiveSettings", ConfigDataType.GameWorld));

                context.Language = globalConfig.BaseLanguage;
            }

            var possibleWords = ConfigDataCache.GetAll<IDictata>().Where(dict => dict.Language == context.Language && dict.WordType == type && dict.SuitableForUse);

            return possibleWords.OrderByDescending(word => (word.Positional == context.Position ? 5 : 0) + 
                                                           (word.Tense == context.Tense ? 5 : 0) + 
                                                           (word.Perspective == context.Perspective ? 5 : 0) +
                                                           (word.Possessive == context.Possessive ? 7 : 0) +
                                                           ((context.GenderForm == null || word.Feminine == context.GenderForm?.Feminine) ? 10 : 0) +
                                                           (word.Plural == context.Plural ? 2 : 0) +
                                                           (word.Determinant == context.Determinant ? 2 : 0) +
                                                           (context.Semantics.Any() ? word.Semantics.Count(wrd => context.Semantics.Contains(wrd)) * 10 : 0)
                                                  ).FirstOrDefault();

            //3x weight for meeting the semantics
        }

        public static IDictata GetAntonym(IDictata baseWord, LexicalContext context)
        {
            if (baseWord == null)
                return baseWord;

            return FocusFindWord(baseWord.Antonyms.AsEnumerable(), context, baseWord);
        }

        public static IDictata GetSynonym(IDictata baseWord, LexicalContext context)
        {
            if (baseWord == null)
                return baseWord;

            return FocusFindWord(baseWord.Synonyms.AsEnumerable(), context, baseWord);
        }

        private static IDictata FocusFindWord(IEnumerable<IDictata> possibleWords, LexicalContext context, IDictata baseWord)
        {
            if (context.Language != null)
            {
                possibleWords = possibleWords.Where(word => word != null && word.Language == context.Language && word.SuitableForUse);
            }

            possibleWords = possibleWords.Where(word => word.Possessive == context.Possessive
                                                            && word.Feminine == context.GenderForm?.Feminine
                                                            && word.Plural == context.Plural
                                                            && word.Determinant == context.Determinant
                                                            && !context.Semantics.Any() || word.Semantics.All(wrd => context.Semantics.Contains(wrd))
                                                            && (context.Position == LexicalPosition.None || word.Positional == context.Position)
                                                            && (context.Tense == LexicalTense.None || word.Tense == context.Tense)
                                                            && (context.Perspective == NarrativePerspective.None || word.Perspective == context.Perspective));

            if (!possibleWords.Any() ||
                (context.Severity + context.Elegance + context.Quality == 0 && baseWord.Language == context.Language && baseWord.Possessive == context.Possessive
                    && baseWord.Feminine == context.GenderForm?.Feminine && baseWord.Plural == context.Plural && baseWord.Determinant == context.Determinant)
                )
            {
                return baseWord;
            }

            return GetRelatedWord(baseWord, context.Severity, context.Elegance, context.Quality, context.Language, possibleWords);
        }

        private static IDictata GetRelatedWord(IDictata baseWord, int severityModifier, int eleganceModifier, int qualityModifier, ILanguage language, IEnumerable<IDictata> possibleWords)
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
    }
}
