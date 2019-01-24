using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural.PropertyBinding;
using NetMud.DataStructure.Inanimate;

namespace NetMud.Data.Architectural.PropertyBinding
{
    public class InanimateTemplateDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            string stringInput = input.ToString();
            if (string.IsNullOrWhiteSpace(stringInput))
                return null;

            return TemplateCache.Get<IInanimateTemplate>(long.Parse(stringInput));
        }
    }
}
