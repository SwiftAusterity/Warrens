using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Architectural.PropertyBinding;

namespace NetMud.Data.Architectural.PropertyBinding
{
    public class RaceDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            string stringInput = input.ToString();
            if (string.IsNullOrWhiteSpace(stringInput))
            {
                return null;
            }

            return TemplateCache.Get<IRace>(long.Parse(stringInput));
        }
    }
}
