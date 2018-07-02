using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Linguistic;
using System.Linq;

namespace NetMud.Communication.Lexicon
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
            VerifyDictata(lexica.GetDictata());
        }

        /// <summary>
        /// Verify the dictionary has this word already
        /// </summary>
        /// <param name="dictata">dictata to check</param>
        public static void VerifyDictata(IDictata dictata)
        {
            var cacheKey = new ConfigDataCacheKey(dictata);

            var maybeDictata = ConfigDataCache.Get<IDictata>(cacheKey);

            if (maybeDictata != null)
                dictata = maybeDictata;

            //Set the language to default if it is absent and save it, if it has a language it already exists
            if (dictata.Language == null)
            {
                //TODO: WorldConfig so base language can be set
                var baseLanguage = ConfigDataCache.GetAll<ILanguage>().FirstOrDefault(lang => lang.SuitableForUse);

                if (baseLanguage != null)
                    dictata.Language = baseLanguage;

                dictata.SystemSave();
            }
        }
    }
}
