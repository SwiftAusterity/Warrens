using NetMud.DataStructure.Behaviors.Automation;

namespace NetMud.DataStructure.Behaviors.Actionable
{
    /// <summary>
    /// For commands/events, inanimate qualifies for crafting/spell/command materials
    /// </summary>
    /// <typeparam name="ICraftingType">the type of crafting material this counts as</typeparam>
    public interface IQualifyAs<ICraftingType>
    {
    }
}
