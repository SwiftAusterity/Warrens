using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.PropertyBinding;
using NetMud.DataStructure.Linguistic;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.Data.Architectural.PropertyBinding
{
    public class DictataPhraseCollectionDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            if (input == null)
            {
                return null;
            }

            IEnumerable<string> valueCollection = input as IEnumerable<string>;

            HashSet<IDictataPhrase> collective = new HashSet<IDictataPhrase>(valueCollection.Select(str => ConfigDataCache.Get<IDictataPhrase>(new ConfigDataCacheKey(typeof(IDictataPhrase), str, ConfigDataType.Dictionary))));

            return collective;
        }
    }
}
