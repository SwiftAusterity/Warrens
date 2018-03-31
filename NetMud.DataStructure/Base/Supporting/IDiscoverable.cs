using NetMud.DataStructure.Base.System;

namespace NetMud.DataStructure.Base.Supporting
{
    /// <summary>
    /// For zones and locales to hide themselves until a player discovers them naturally
    /// </summary>
    public interface IDiscoverable
    {
        /// <summary>
        /// Does this even need to be discovered?
        /// </summary>
        bool AlwaysVisible { get; set; }

        /// <summary>
        /// What "Achievement" to look for to make this visible
        /// </summary>
        string DiscoveryName { get; }

        /// <summary>
        /// Check if this entity can see this via pathways or not
        /// </summary>
        /// <param name="discoverer">The entity being questioned</param>
        /// <returns>Whether or not the entity should be able to see the pathway to this</returns>
        bool IsDiscovered(IEntity discoverer);
    }
}
