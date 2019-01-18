using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.PropertyBinding;
using System.Collections.Generic;

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
