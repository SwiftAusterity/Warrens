using NetMud.DataStructure.Architectural.EntityBase;

namespace NetMud.DataStructure.Player
{
    /// <summary>
    /// Backing data for player characters
    /// </summary>
    public interface IPlayerTemplate : ITemplate, IPlayerFramework
    {
        /// <summary>
        /// What account owns this character
        /// </summary>
        IAccount Account { get; }

        /// <summary>
        /// Last known location Id for character in live world
        /// </summary>
        ulong CurrentSlice { get; set; }

        /// <summary>
        /// Given name + surname
        /// </summary>
        /// <returns></returns>
        string FullName();
    }
}
