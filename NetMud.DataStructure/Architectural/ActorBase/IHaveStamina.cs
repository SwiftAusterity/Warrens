namespace NetMud.DataStructure.Architectural.ActorBase
{
    /// <summary>
    /// Entity can use the Sleep command, generally paired with IGetTired
    /// </summary>
    public interface IHaveStamina
    {
        /// <summary>
        /// How much total stamina one has
        /// </summary>
        int TotalStamina { get; set; }
    }
}
