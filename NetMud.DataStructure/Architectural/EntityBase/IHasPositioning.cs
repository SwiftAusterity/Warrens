namespace NetMud.DataStructure.Architectural.EntityBase
{
    /// <summary>
    /// Var collection for IExist
    /// </summary>
    public interface IHasPositioning : IRenderInLocation
    {
        /// <summary>
        /// position in the worlds
        /// </summary>
        IGlobalPosition CurrentLocation { get; }

        /// <summary>
        /// Change the position of this
        /// </summary>
        /// <param name="direction">the 0-360 direction we're moving</param>
        /// <param name="newPosition">The new position the thing is in, will return with the original one if nothing moved</param>
        /// <returns>an error</returns>
        string TryMoveTo(IGlobalPosition newPosition);

        /// <summary>
        /// Change the position of this without physical movement
        /// </summary>
        /// <param name="newPosition">The new position the thing is in, will return with the original one if nothing moved</param>
        /// <returns>was this thing moved?</returns>
        string TryTeleport(IGlobalPosition newPosition);
    }
}
