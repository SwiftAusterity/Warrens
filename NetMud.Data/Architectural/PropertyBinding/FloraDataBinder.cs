using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural.PropertyBinding;
using NetMud.DataStructure.NaturalResource;

namespace NetMud.Data.Architectural.PropertyBinding
{
    public class FloraDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            var stringInput = input.ToString();
            if (string.IsNullOrWhiteSpace(stringInput))
                return null;

            return TemplateCache.Get<IFlora>(long.Parse(stringInput));
        }
    }
}
