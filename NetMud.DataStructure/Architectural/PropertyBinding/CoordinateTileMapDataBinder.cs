using System.Collections.Generic;
using System.Linq;

namespace NetMud.DataStructure.Architectural.PropertyBinding
{
    public class CoordinateTileMapDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            if (input == null)
                return null;

            long[,] coordinateGrouping = new long[100,100];
            var inputArray = input as IEnumerable<string>;
            int maxNodes = inputArray.Count() - 1;
            short x = 0;
            short y = 99;

            for (int i = 0; i < maxNodes; i++)
            {
                coordinateGrouping[x, y] = long.Parse(inputArray.ElementAt(i));

                x++;
                if (x > 99)
                {
                    y--;
                    x = 0;
                }

                if (y < 0)
                    break;
            }

            return coordinateGrouping;
        }
    }
}
