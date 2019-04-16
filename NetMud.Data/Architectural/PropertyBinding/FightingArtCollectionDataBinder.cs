using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural.PropertyBinding;
using NetMud.DataStructure.Combat;
using System.Collections.Generic;

namespace NetMud.Data.Architectural.PropertyBinding
{
    public class FightingArtCollectionDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            string[] keys = (string[])input;

            SortedSet<IFightingArt> set = new SortedSet<IFightingArt>();

            foreach(var key in keys)
            {
                var art = TemplateCache.Get<IFightingArt>(long.Parse(key));

                if(art != null)
                {
                    set.Add(art);
                }
            }

            return set;
        }
    }
}
