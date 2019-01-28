using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural.PropertyBinding;
using NetMud.DataStructure.NaturalResource;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.Data.Architectural.PropertyBinding
{
    public class MineralCollectionDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            if (input == null)
            {
                return null;
            }

            IEnumerable<string> valueCollection = input as IEnumerable<string>;

            HashSet<IMineral> collective = new HashSet<IMineral>(valueCollection.Select(str => TemplateCache.Get<IMineral>(long.Parse(str))));

            return collective;
        }
    }
}
