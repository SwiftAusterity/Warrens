namespace NetMud.DataStructure.Architectural.ActorBase
{
    /// <summary>
    /// Indicates an entity is subject to becoming tired from doing things
    /// </summary>
    public interface IGetTired : IHaveStamina
    {
        /// <summary>
        /// How much stamina you currently have
        /// </summary>
        int CurrentStamina { get; set; }

        /// <summary>
        /// Take a hit to stamina
        /// </summary>
        /// <param name="exhaustionAmount">How much exhaustion is happening</param>
        /// <returns>new current stamina</returns>
        int Exhaust(int exhaustionAmount);

        /// <summary>
        /// Process sleeping/resting
        /// </summary>
        /// <param name="hours">How many hours we slept</param>
        /// <returns>New current stamina</returns>
        int Sleep(int hours);
    }
}
