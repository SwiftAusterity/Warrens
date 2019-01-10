namespace NetMud.DataStructure.Architectural.ActorBase
{
    /// <summary>
    /// This mobile can recieve audible notification messages and triggers
    /// </summary>
    public interface ICanTouch
    {
        /// <summary>
        /// Gets the actual modifier taking into account deafness and other factors
        /// </summary>
        /// <returns>the working modifier</returns>
        ValueRange<float> GetTactileRange();
    }
}
