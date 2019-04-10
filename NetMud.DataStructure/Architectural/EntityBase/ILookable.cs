namespace NetMud.DataStructure.Architectural.EntityBase
{
    /// <summary>
    /// Framework for rendering output when this entity is Look(ed) at
    /// </summary>
    public interface ILookable
    {
        /// <summary>
        /// Renders output for this entity when Look targets it
        /// </summary>
        /// <param name="viewer">entity initiating the command</param>
        /// <returns>the output</returns>
        string RenderToVisible(IEntity viewer);
    }
}
