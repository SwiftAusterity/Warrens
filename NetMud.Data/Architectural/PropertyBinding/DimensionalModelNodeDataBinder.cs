using NetMud.Data.Architectural.EntityBase;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Architectural.PropertyBinding;
using System;
using System.Collections.Generic;

namespace NetMud.Data.Architectural.PropertyBinding
{
    public class DimensionalModelNodeDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            HashSet<IDimensionalModelNode> nodes = new HashSet<IDimensionalModelNode>();
            IEnumerable<string> nodeValues = input as IEnumerable<string>;

            short i = 0;
            foreach(string nodeValue in nodeValues)
            {
                nodes.Add(new DimensionalModelNode() { XAxis = i, Style = (DamageType)Enum.Parse(typeof(DamageType), nodeValue) });
                i++;
            }

            return nodes;
        }
    }
}
