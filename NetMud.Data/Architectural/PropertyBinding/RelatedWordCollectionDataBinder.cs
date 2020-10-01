using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.PropertyBinding;
using NetMud.DataStructure.Linguistic;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.Data.Architectural.PropertyBinding
{
    public class RelatedWordCollectionDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            if (input == null)
            {
                return null;
            }

            IEnumerable<string> valueCollection = input as IEnumerable<string>;

            HashSet<IRelatedWord> collective = new HashSet<IRelatedWord>(valueCollection.SelectMany(str => ConfigDataCache.Get<ILexeme>(new ConfigDataCacheKey(typeof(ILexeme), ConfigDataType.Dictionary, str))?.WordForms.SelectMany(wf => wf.RelatedWords)));

            return collective;
        }
    }
}
