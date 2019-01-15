using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural.PropertyBinding;
using NetMud.DataStructure.Room;

namespace NetMud.Data.Architectural.PropertyBinding
{
    public class PathwayTemplateDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            var stringInput = input.ToString();
            if (string.IsNullOrWhiteSpace(stringInput))
                return null;

            return TemplateCache.Get<IPathwayTemplate>(long.Parse(stringInput));
        }
    }
}
