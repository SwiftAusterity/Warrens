using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural.PropertyBinding;
using NetMud.DataStructure.Tile;

namespace NetMud.Data.Architectural.PropertyBinding
{
    public class TileTemplateDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            var stringInput = input.ToString();
            if (string.IsNullOrWhiteSpace(stringInput))
                return null;

            return TemplateCache.Get<ITileTemplate>(long.Parse(stringInput));
        }
    }
}
