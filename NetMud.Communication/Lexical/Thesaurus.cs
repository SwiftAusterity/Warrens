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
            catch(Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return string.Empty;
        }

        public static IDictata GetSynonym(IDictata baseWord, int severityModifier, int eleganceModifier, int qualityModifier, ILanguage language = null)
        {
            if (baseWord == null)
                return baseWord;

            var possibleWords = baseWord.Synonyms.AsEnumerable();

            if (language != null)
            {
                possibleWords = possibleWords.Where(word => word.Language == language);
            }

            if (!possibleWords.Any())
            {
                return baseWord;
            }

            var rankedWords = new Dictionary<IDictata, int>();
            foreach (var word in possibleWords)
            {
                int rating = 0;

                rating += Math.Abs((baseWord.Severity + severityModifier) - word.Severity);
                rating += Math.Abs((baseWord.Elegance + eleganceModifier) - word.Elegance);
                rating += Math.Abs((baseWord.Quality + qualityModifier) - word.Quality);

                rankedWords.Add(word, rating);
            }

            var closestWord = rankedWords.OrderBy(pair => pair.Value).FirstOrDefault();

            return closestWord.Key ?? baseWord;
        }
    }
}
