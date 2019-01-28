using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural.PropertyBinding;
using NetMud.DataStructure.NPC;

namespace NetMud.Data.Architectural.PropertyBinding
{
    public class NonPlayerCharacterTemplateDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            string stringInput = input.ToString();
            if (string.IsNullOrWhiteSpace(stringInput))
            {
                return null;
            }

            return TemplateCache.Get<INonPlayerCharacterTemplate>(long.Parse(stringInput));
        }
    }
}
