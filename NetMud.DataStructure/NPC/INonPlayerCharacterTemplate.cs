using NetMud.DataStructure.Architectural.EntityBase;

namespace NetMud.DataStructure.NPC
{
    /// <summary>
    /// Backing data for NPC/NonPlayerCharacters
    /// </summary>
    public interface INonPlayerCharacterTemplate : ITemplate, INonPlayerCharacterFramework
    {
        /// <summary>
        /// Given + family name for NPCs
        /// </summary>
        /// <returns></returns>
        string FullName();
    }
}
