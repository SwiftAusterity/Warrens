using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural.PropertyBinding;
using NetMud.DataStructure.Locale;

namespace NetMud.Data.Architectural.PropertyBinding
{
    public class LocaleTemplateDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            var stringInput = input.ToString();
            if (string.IsNullOrWhiteSpace(stringInput))
                return null;

            return TemplateCache.Get<ILocaleTemplate>(long.Parse(stringInput));
        }
    }
}
