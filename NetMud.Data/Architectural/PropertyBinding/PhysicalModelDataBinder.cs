using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.PropertyBinding;
using NetMud.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.Data.Architectural.PropertyBinding
{
    public class PhysicalModelDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            if (input == null)
                return null;

            var coordinateGrouping = input as IEnumerable<string>;
            var returnList = new HashSet<Coordinate>();

            for (var y = 21; y >= 0; y--)
            {
            }

            return returnList;
        }
    }
}
