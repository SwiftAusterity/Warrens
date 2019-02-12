using NetMud.DataAccess;
using NetMud.DataStructure.Linguistic;
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

        public static IDictata GetAntonym(IDictata baseWord, int severityModifier, int eleganceModifier, int qualityModifier,
            bool possessive, bool feminine, bool plural, bool determinant, LexicalPosition positioning, LexicalTense tense, NarrativePerspective perspective, ILanguage language = null)
        {
            if (baseWord == null)
                return baseWord;

            var possibleWords = baseWord.Antonyms.AsEnumerable();

            if (language != null)
            {
                possibleWords = possibleWords.Where(word => word.Language == language);
            }

            possibleWords = possibleWords.Where(word => word.Possessive == possessive
                                                            && word.Feminine == feminine
                                                            && word.Plural == plural
                                                            && word.Determinant == determinant
                                                            && (positioning == LexicalPosition.None || word.Positional == positioning)
                                                            && (tense == LexicalTense.None || word.Tense == tense)
                                                            && (perspective == NarrativePerspective.None || word.Perspective == perspective)
                                                            && word.SuitableForUse);

            if (!possibleWords.Any() ||
                (severityModifier + eleganceModifier + qualityModifier == 0 && baseWord.Language == language && baseWord.Possessive == possessive
                    && baseWord.Feminine == feminine && baseWord.Plural == plural && baseWord.Determinant == determinant)
                )
            {
                return baseWord;
            }

            return GetRelatedWord(baseWord, severityModifier, eleganceModifier, qualityModifier, language, possibleWords);
        }


        public static IDictata GetSynonym(IDictata baseWord, LexicalContext context)
        {
            if (baseWord == null)
                return baseWord;

            var possibleWords = baseWord.Synonyms.AsEnumerable();

            if (context.Language != null)
            {
                possibleWords = possibleWords.Where(word => word.Language == context.Language);
            }

            possibleWords = possibleWords.Where(word => word.Possessive == context.Possessive
                                                            && word.Feminine == context.GenderForm?.Feminine
                                                            && word.Plural == context.Plural
                                                            && word.Determinant == context.Determinant
                                                            && (context.Position == LexicalPosition.None || word.Positional == context.Position)
                                                            && (context.Tense == LexicalTense.None || word.Tense == context.Tense)
                                                            && (context.Perspective == NarrativePerspective.None || word.Perspective == context.Perspective)
                                                            && word.SuitableForUse);

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
