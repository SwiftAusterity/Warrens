using NetMud.DataAccess.Cache;
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

            return ConfigDataCache.Get<ILanguage>(stringInput);
        }
    }
}
