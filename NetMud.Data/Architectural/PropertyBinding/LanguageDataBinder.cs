using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.PropertyBinding;
using NetMud.DataStructure.Linguistic;

namespace NetMud.Data.Architectural.PropertyBinding
{
    public class LanguageDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            string stringInput = input.ToString();
            if (string.IsNullOrWhiteSpace(stringInput))
            {
                return null;
            }

            ILanguage returnValue = ConfigDataCache.Get<ILanguage>(new ConfigDataCacheKey(typeof(ILanguage), stringInput, ConfigDataType.Language));

            return returnValue;
        }
    }
}
