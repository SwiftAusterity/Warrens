using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.PropertyBinding;
using NetMud.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.Data.Architectural.PropertyBinding
{
    public class AffectPatternDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            if (input == null)
                return null;

            IEnumerable<string> coordinateGrouping = input as IEnumerable<string>;
            HashSet<Coordinate> returnList = new HashSet<Coordinate>();

            for (int y = 10; y >= 0; y--)
            {
                for(int x = 0; x <= 10; x++)
                {
                    bool isChecked = coordinateGrouping.ElementAt(y).Split(",", StringSplitOptions.RemoveEmptyEntries)[x] == "1";

                    if (isChecked)
                        returnList.Add(new Coordinate(x, y, -1));
                }
            }

            return returnList;
        }
    }
}
