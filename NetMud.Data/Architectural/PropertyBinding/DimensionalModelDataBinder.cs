using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Architectural.PropertyBinding;

namespace NetMud.Data.Architectural.PropertyBinding
{
    public class DimensionalModelDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            string stringInput = input.ToString();
            if (string.IsNullOrWhiteSpace(stringInput))
            {
                return null;
            }

            return TemplateCache.Get<IDimensionalModelData>(long.Parse(stringInput));
        }
    }
}
