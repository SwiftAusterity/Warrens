namespace NetMud.DataStructure.Architectural.ActorBase
{
    /// <summary>
    /// This can take damage
    /// </summary>
    public interface ICanBeHarmed : IHaveHealth
    {
        /// <summary>
        /// How much hp you currently have
        /// </summary>
        int CurrentHealth { get; set; }

        /// <summary>
        /// Take a hit to hp
        /// </summary>
        /// <param name="damage">How much damage is happening</param>
        /// <returns>new current hp</returns>
        int Harm(int damage);

        /// <summary>
        /// Process recovering hp
        /// </summary>
        /// <param name="recovery">How much hp we are recovering</param>
        /// <returns>New current health</returns>
        int Recover(int recovery);
    }
}
