using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.PropertyBinding;
using NetMud.DataStructure.Linguistic;

namespace NetMud.Data.Architectural.PropertyBinding
{
    public class DictataPhraseDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            string stringInput = input.ToString();
            if (string.IsNullOrWhiteSpace(stringInput))
            {
                return null;
            }

            IDictataPhrase returnValue = ConfigDataCache.Get<IDictataPhrase>(new ConfigDataCacheKey(typeof(IDictataPhrase), stringInput, ConfigDataType.Dictionary));

            return returnValue;
        }
    }
}
