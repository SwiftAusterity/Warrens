using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.PropertyBinding;
using NetMud.DataStructure.Linguistic;
using System.Linq;

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

            IDictata returnValue = ConfigDataCache.Get<ILexeme>(new ConfigDataCacheKey(typeof(ILexeme), ConfigDataType.Dictionary, stringInput))?.WordForms.FirstOrDefault();

            return returnValue;
        }
    }
}
