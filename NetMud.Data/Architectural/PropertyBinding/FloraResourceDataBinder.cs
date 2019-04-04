using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural.PropertyBinding;
using NetMud.DataStructure.NaturalResource;

namespace NetMud.Data.Architectural.PropertyBinding
{
    public class FloraResourceBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            string stringInput = input.ToString();
            if (string.IsNullOrWhiteSpace(stringInput))
            {
                return null;
            }

            long id = long.Parse(stringInput);

            return TemplateCache.Get<IFlora>(id);
        }
    }
}
