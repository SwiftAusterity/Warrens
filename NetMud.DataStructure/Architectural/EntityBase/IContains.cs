using NetMud.DataStructure.Inanimate;
using System.Collections.Generic;

namespace NetMud.DataStructure.Architectural.EntityBase
{
    /// <summary>
    /// Framework for entities that can hold other entities
    /// </summary>
    public interface IContains : IEntity
    {
        /// <summary>
        /// Total item capacity of the container
        /// </summary>
        int Capacity { get; }

        /// <summary>
        /// Move an entity into this
        /// </summary>
        /// <typeparam name="T">the type of the entity to add</typeparam>
        /// <param name="thing">the entity to add</param>
        /// <returns>errors</returns>
        string MoveInto<T>(T thing);

        /// <summary>
        /// Move an entity out of this
        /// </summary>
        /// <typeparam name="T">the type of entity to remove</typeparam>
        /// <param name="thing">the entity</param>
        /// <returns>errors</returns>
        string MoveFrom<T>(T thing);

        /// <summary>
        /// Get all of the entities matching a type inside this
        /// </summary>
        /// <typeparam name="T">the type</typeparam>
        /// <returns>the contained entities</returns>
        IEnumerable<T> GetContents<T>();

        /// <summary>
        /// Show the stacks in this container, only for inanimates
        /// </summary>
        /// <returns>A list of the item stacks</returns>
        HashSet<IItemStack> ShowStacks(IEntity observer);

        /// <summary>
        /// Returns this entity as a container position
        /// </summary>
        /// <returns></returns>
        IGlobalPosition GetContainerAsLocation();
    }
}
