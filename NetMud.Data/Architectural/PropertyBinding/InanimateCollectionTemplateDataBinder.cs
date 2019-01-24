using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural.PropertyBinding;
using NetMud.DataStructure.Inanimate;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.Data.Architectural.PropertyBinding
{
    public class InanimateCollectionTemplateDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            if (input == null)
                return null;

            IEnumerable<string> valueCollection = input as IEnumerable<string>;

            HashSet<IInanimateTemplate> collective = new HashSet<IInanimateTemplate>(valueCollection.Select(str => TemplateCache.Get<IInanimateTemplate>(long.Parse(str))));

            return collective;
        }
    }
}
