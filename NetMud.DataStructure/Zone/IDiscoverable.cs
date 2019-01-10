using NetMud.DataStructure.Architectural.EntityBase;

namespace NetMud.DataStructure.Zone
{
    /// <summary>
    /// For zones and locales to hide themselves until a player discovers them naturally
    /// </summary>
    public interface IDiscoverable
    {
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
