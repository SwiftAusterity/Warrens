using NetMud.DataStructure.Behaviors.Actionable;
namespace NetMud.DataStructure.Behaviors.Automation
{
    /// <summary>
    /// Indicates an entity is subject to becoming tired (with ill effects), pairs with ISleep
    /// </summary>
    public interface IGetTired : ICanSleep
    {
        int Somnus { get; set; }
    }
}
