using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural.PropertyBinding;

namespace NetMud.Data.Architectural.PropertyBinding
{
    public class HelpDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            string stringInput = input.ToString();
            if (string.IsNullOrWhiteSpace(stringInput))
            {
                return null;
            }

            return TemplateCache.Get<IHelp>(long.Parse(stringInput));
        }
    }
}
