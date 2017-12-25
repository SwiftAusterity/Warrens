namespace NetMud.DataStructure.Behaviors.Actionable
{
    /// <summary>
    /// Handles experience storage and some methods for adding/removing
    /// </summary>
    public interface ICanGainExperience
    {
        /// <summary>
        /// How much experience is gained by unbanked (short term memory)
        /// </summary>
        long GainedExperience { get; set; }

        /// <summary>
        /// How much experience is banked
        /// </summary>
        long BankedExperience { get; set; }

        /// <summary>
        /// How much acquired experience goes towards unbanked (remainder goes to banked)
        /// </summary>
        float UnbankedExperienceGainSplit { get; set; }

        /// <summary>
        /// Spends experience, hits banked first then gained, returns FALSE if there's not enough
        /// </summary>
        /// <param name="wantingToSpend">how much we want to spend</param>
        /// <returns>if we had enough or not</returns>
        bool SpendExperience(long wantingToSpend);

        /// <summary>
        /// Drains experience. Can drain partial, will return the remainder not drained if we hit zero
        /// </summary>
        /// <param name="wantingToLose">how much we want to lose</param>
        /// <returns>0 or the remainder if we hit the floor early</returns>
        long LoseExperience(long wantingToLose);

        /// <summary>
        /// Adds experience
        /// </summary>
        /// <param name="wantingToAcquire">how much we want to add</param>
        /// <returns>if adding was successful</returns>
        bool AcquireExperience(long wantingToAcquire);

        /// <summary>
        /// Banks gained experience
        /// </summary>
        void BankExperience();
    }
}
