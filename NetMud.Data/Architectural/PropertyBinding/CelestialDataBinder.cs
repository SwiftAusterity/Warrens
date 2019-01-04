using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural.PropertyBinding;
using NetMud.DataStructure.Gaia;

namespace NetMud.Data.Architectural.PropertyBinding
{
    public class CelestialDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            var stringInput = input.ToString();
            if (string.IsNullOrWhiteSpace(stringInput))
                return null;

            return TemplateCache.Get<ICelestial>(long.Parse(stringInput));
        }
    }
}
