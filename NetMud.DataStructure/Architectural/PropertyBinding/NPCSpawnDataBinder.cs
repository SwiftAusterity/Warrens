using NetMud.DataStructure.Zone;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.DataStructure.Architectural.PropertyBinding
{
    public class NPCSpawnDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            if (input == null)
                return null;

            var inputArray = input as IEnumerable<string>;
            int maxNodes = inputArray.Count() - 1;
            short x = 0;
            short y = 99;

            HashSet<NPCSpawn> spawns = new HashSet<NPCSpawn>();
            for (int i = 0; i < maxNodes; i++)
            {
                if (inputArray.Count() > i && long.Parse(inputArray.ElementAt(i)) > -1)
                {
                    var itemId = long.Parse(inputArray.ElementAt(i));
                    var newThingType = new NPCSpawn()
                    {
                        NPCId = itemId,
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
