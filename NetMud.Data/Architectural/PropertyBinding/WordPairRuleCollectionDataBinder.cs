using NetMud.DataStructure.Architectural.PropertyBinding;
using NetMud.DataStructure.Linguistic;
using System.Collections.Generic;

namespace NetMud.Data.Architectural.PropertyBinding
{
    public class WordPairRuleCollectionDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            if (input == null)
            {
                return null;
            }

            IEnumerable<string> valueCollection = input as IEnumerable<string>;

            HashSet<IWordPairRule> collective = new HashSet<IWordPairRule>();

            return collective;
        }
    }
}
