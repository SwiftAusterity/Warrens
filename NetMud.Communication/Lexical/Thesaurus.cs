using Google.Cloud.Translation.V2;
using NetMud.DataStructure.Linguistic;

namespace NetMud.Communication.Lexical
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
