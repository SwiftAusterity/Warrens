using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.SupportingClasses;

namespace NetMud.DataStructure.Behaviors.Existential
{
    /// <summary>
    /// Encapsulates position in the world
    /// </summary>
    public interface IExist : IHasPositioning, ILookable, IAudible, ISmellable, ISensible, IInspectable, IScanable, ITrackable, ITasteable, ITouchable
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
        /// Get the current luminosity rating of the place you're in
        /// </summary>
        /// <returns>The current Luminosity</returns>
        float GetCurrentLuminosity();

        /// <summary>
        /// A fully described short description (includes adjectives)
        /// </summary>
        /// <param name="viewer">entity initiating the command</param>
        /// <returns>the output</returns>
        IOccurrence GetFullDescription(IEntity viewer);

        /// <summary>
        /// A fully described short description (includes adjectives)
        /// </summary>
        /// <param name="viewer">entity initiating the command</param>
        /// <returns>the output</returns>
        IOccurrence GetImmediateDescription(IEntity viewer);

        /// <summary>
        /// The name of a thing based on visual description
        /// </summary>
        /// <param name="viewer">Who is looking</param>
        /// <returns>a string of the name</returns>
        string GetDescribableName(IEntity viewer);
    }
}
