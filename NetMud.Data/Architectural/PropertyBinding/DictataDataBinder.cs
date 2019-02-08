using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.PropertyBinding;
using NetMud.DataStructure.Linguistic;

namespace NetMud.Data.Architectural.PropertyBinding
{
    public class DictataDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            string stringInput = input.ToString();
            if (string.IsNullOrWhiteSpace(stringInput))
            {
                return null;
            }

            var returnValue = ConfigDataCache.Get<IDictata>(new ConfigDataCacheKey(typeof(IDictata), stringInput, ConfigDataType.Dictionary));

            return returnValue;
        }
    }
}
