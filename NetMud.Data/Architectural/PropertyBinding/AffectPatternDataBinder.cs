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

            var coordinateGrouping = input as IEnumerable<string>;
            var returnList = new HashSet<Coordinate>();

            for (var y = 10; y >= 0; y--)
            {
                for(var x = 0; x <= 10; x++)
                {
                    var isChecked = coordinateGrouping.ElementAt(y).Split(",", StringSplitOptions.RemoveEmptyEntries)[x] == "1";

                    if (isChecked)
                        returnList.Add(new Coordinate(x, y, -1));
                }
            }

            return returnList;
        }
    }
}
