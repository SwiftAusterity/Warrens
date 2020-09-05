using NetMud.DataAccess.Cache;
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

            IDictata returnValue = LuceneDataCache.Get<ILexeme>(new LuceneDataCacheKey(typeof(ILexeme), stringInput))?.WordForms.FirstOrDefault();

            return returnValue;
        }
    }
}
