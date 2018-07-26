using Google.Apis.Translate.v2;
using Google.Apis.Translate.v2.Data;
using Google.Cloud.Translation.V2;
using NetMud.DataStructure.Linguistic;
using System.Linq;

namespace NetMud.Data.Lexical
{
    /// <summary>
    /// Finds synonyms and antonyms
    /// </summary>
    public static class Thesaurus
    {
        public static string GetTranslatedWord(string phrase, ILanguage targetLanguage)
        {
            //TODO: figure out google credentials
            TranslationClient client = TranslationClient.Create();
            var response = client.TranslateText(phrase, targetLanguage.GoogleLanguageCode);

            return response.TranslatedText;
        }
    }
}
