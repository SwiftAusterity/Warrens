using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.PropertyBinding;

namespace NetMud.Data.Architectural.PropertyBinding
{
    public class MarkdownBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            string stringInput = input.ToString();
            if (string.IsNullOrWhiteSpace(stringInput))
            {
                return null;
            }

            return new MarkdownString(stringInput);
        }
    }
}
