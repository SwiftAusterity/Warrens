using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural.PropertyBinding;
using NetMud.DataStructure.Combat;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.Data.Architectural.PropertyBinding
{
    public class FightingArtCollectionDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            if (input == null)
                return null;

            var valueCollection = input as IEnumerable<string>;

            var collective = new SortedSet<IFightingArt>(valueCollection.Select(str => TemplateCache.Get<IFightingArt>(long.Parse(str))));

            return collective;
        }
    }
}
