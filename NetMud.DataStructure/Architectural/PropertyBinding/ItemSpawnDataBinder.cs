using NetMud.DataStructure.Zone;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.DataStructure.Architectural.PropertyBinding
{
    public class ItemSpawnDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            if (input == null)
                return null;

            var inputArray = input as IEnumerable<string>;
            int maxNodes = inputArray.Count() - 1;
            short x = 0;
            short y = 99;

            HashSet<InanimateSpawn> spawns = new HashSet<InanimateSpawn>();
            for (int i = 0; i < maxNodes; i++)
            {
                if (inputArray.Count() > i && long.Parse(inputArray.ElementAt(i)) > -1)
                {
                    var itemId = long.Parse(inputArray.ElementAt(i));
                    var newThingType = new InanimateSpawn()
                    {
                        ItemId = itemId,
                        Placement = new Coordinate(x, y)
                    };

                    spawns.Add(newThingType);
                }

                x++;
                if (x > 99)
                {
                    y--;
                    x = 0;
                }

                if (y < 0)
                    break;
            }

            return spawns;
        }
    }
}
