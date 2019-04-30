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
    }
}
