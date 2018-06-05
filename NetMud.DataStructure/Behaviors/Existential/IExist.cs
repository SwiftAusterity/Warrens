using NetMud.DataStructure.Base.System;

namespace NetMud.DataStructure.Behaviors.Existential
{
    /// <summary>
    /// Encapsulates position in the world
    /// </summary>
    public interface IExist : IHasPositioning
    {
        /// <summary>
        /// Handles returning container's position if inside of something
        /// </summary>
        /// <returns>positional coordinates</returns>
        IGlobalPosition AbsolutePosition();

        /// <summary>
        /// Spawns a new instance of this entity in the live world into a default position
        /// </summary>
        void SpawnNewInWorld();

        /// <summary>
        /// Spawn a new instance of this entity into the live world in a set position
        /// </summary>
        /// <param name="position">container and zone to spawn to</param>
        void SpawnNewInWorld(IGlobalPosition position);

        /// <summary>
        /// Is this thing visible to the viewing entity
        /// </summary>
        /// <param name="viewer">the viewing entity</param>
        /// <returns>If this is visible</returns>
        bool IsVisibleTo(IEntity viewer);
    }
}
