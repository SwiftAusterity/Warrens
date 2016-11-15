namespace NetMud.DataStructure.Behaviors.Automation
{
    /// <summary>
    /// Indicates an entity is subject to becoming hungry (and ill effects)
    /// </summary>
    public interface IHunger : IEat<DietType>
    {
        int Hunger { get; set; }
    }
}
