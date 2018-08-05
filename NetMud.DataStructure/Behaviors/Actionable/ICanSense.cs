using System;

namespace NetMud.DataStructure.Behaviors.Actionable
{
    /// <summary>
    /// This mobile can recieve audible notification messages and triggers
    /// </summary>
    public interface ICanSense
    {
        /// <summary>
        /// Gets the actual modifier taking into account deafness and other factors
        /// </summary>
        /// <returns>the working modifier</returns>
        Tuple<float, float> GetPsychicRange();
    }
}
