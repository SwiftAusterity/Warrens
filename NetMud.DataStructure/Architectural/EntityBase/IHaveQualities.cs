using System.Collections.Generic;

namespace NetMud.DataStructure.Architectural.EntityBase
{
    public interface IHaveQualities
    {
        /// <summary>
        /// List of live qualities of this entity
        /// </summary>
        HashSet<IQuality> Qualities { get; set; }

        /// <summary>
        /// Check for a quality
        /// </summary>
        /// <param name="name">Gets the value of the request quality</param>
        /// <returns>The value</returns>
        int GetQuality(string name);

        /// <summary>
        /// Add a quality (can be negative)
        /// </summary>
        /// <param name="value">The value you're adding</param>
        /// <param name="additive">Is this additive or replace-ive</param>
        /// <returns>The new value</returns>
        int SetQuality(int value, string quality, bool additive);
    }
}
