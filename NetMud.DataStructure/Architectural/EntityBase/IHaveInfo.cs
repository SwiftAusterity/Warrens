

namespace NetMud.DataStructure.Architectural.EntityBase
{
    /// <summary>
    /// Indicates an entity has an info card that can be shown in the client
    /// </summary>
    public interface IHaveInfo
    {
        /// <summary>
        /// Renders HTML for the info card popups
        /// </summary>
        /// <param name="viewer">entity initiating the command</param>
        /// <returns>the output HTML</returns>
        string RenderToInfo(IEntity viewer);
    }
}
