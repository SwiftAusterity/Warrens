using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Linguistic;
using System.Linq;

namespace NetMud.Communication.Lexical
{
    /// <summary>
    /// Processes Lexica and outputs formatted prose
    /// </summary>
    public static class LexicalProcessor
    {
        /// <summary>
        /// Verify the dictionary has this word already
        /// </summary>
        /// <param name="lexica">lexica to check</param>
        public static void VerifyDictata(ILexica lexica)
        {
            if (string.IsNullOrWhiteSpace(lexica.Phrase))
            {
                return;
            }

            //Experiment: make new everything
            if (!VerifyDictata(lexica.GetDictata()))
            {
                //make a new one
                lexica.GenerateDictata();
            }
        }

        /// <summary>
        /// Verify the dictionary has this word already
        /// </summary>
        /// <param name="dictata">dictata to check</param>
        public static bool VerifyDictata(IDictata dictata)
        {
            if (dictata == null || string.IsNullOrWhiteSpace(dictata.Name))
            {
                return false;
            }

            ConfigDataCacheKey cacheKey = new ConfigDataCacheKey(dictata);

            IDictata maybeDictata = ConfigDataCache.Get<IDictata>(cacheKey);

            if (maybeDictata != null)
            {
                if (maybeDictata.Language != null)
                {
                    return true;
                }

                dictata = maybeDictata;
            }

            //Set the language to default if it is absent and save it, if it has a language it already exists
            if (dictata.Language == null)
            {
                //TODO: WorldConfig so base language can be set
                ILanguage baseLanguage = ConfigDataCache.GetAll<ILanguage>().FirstOrDefault(lang => lang.SuitableForUse);

                if (baseLanguage != null)
                {
                    dictata.Language = baseLanguage;
                }
            }

            dictata.SystemSave();

            return true;
        }

        public static string GetPunctuationMark(SentenceType type)
        {
            string punctuation = string.Empty;
            switch (type)
            {
                case SentenceType.Exclamation:
                    punctuation = "!";
                    break;
                case SentenceType.ExclamitoryQuestion:
                    punctuation = "?!";
                    break;
                case SentenceType.Partial:
                    punctuation = ";";
                    break;
                case SentenceType.Question:
                    punctuation = "?";
                    break;
                case SentenceType.Statement:
                    punctuation = ".";
                    break;
            }

            return punctuation;
        }
    }
}
