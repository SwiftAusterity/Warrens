namespace NetMud.DataStructure.Architectural.ActorBase
{
    /// <summary>
    /// For IHunger entities, details what they can eat
    /// </summary>
    public interface IEat
    {
        /// <summary>
        /// What can this eat
        /// </summary>
        DietType Diet { get; set; }
    }
}
