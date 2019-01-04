using NetMud.DataStructure.Architectural.ActorBase;

namespace NetMud.DataStructure.Behaviors.Automation
{
    /// <summary>
    /// Indicates an entity is subject to becoming hungry (and ill effects)
    /// </summary>
    public interface IHunger : IEat
    {
        /// <summary>
        /// Current satiation level
        /// </summary>
        int Satiation { get; set; }

        /// <summary>
        /// Maximum statiation for this
        /// </summary>
        int MaximumSatiation { get; set; }
    }
}
