using System.Collections.Generic;

namespace NetMud.DataStructure.SupportingClasses
{
    /// <summary>
    /// Object that handles any and all "this contains entities" for the system
    /// </summary>
    /// <typeparam name="T">the type of entities it can contain</typeparam>
    public interface IEntityContainer<T>
    {
        /// <summary>
        /// How large is this container
        /// </summary>
        long CapacityVolume { get; set; }

        /// <summary>
        /// How much weight can it carry before taking damage
        /// </summary>
        long CapacityWeight { get; set; }

        /// <summary>
        /// Restful list of entities contained (it needs to never store its own objects, only cache references)
        /// </summary>
        IEnumerable<T> EntitiesContained { get; }

        /// <summary>
        /// Add an entity to this
        /// </summary>
        /// <param name="entity">the entity to add</param>
        /// <returns>success status</returns>
        bool Add(T entity);

        /// <summary>
        /// Does this contain the specified entity
        /// </summary>
        /// <param name="entity">the entity in question</param>
        /// <returns>yes it contains it or no it does not</returns>
        bool Contains(T entity);

        /// <summary>
        /// Remove an entity from this
        /// </summary>
        /// <param name="entity">the entity to remove</param>
        /// <returns>success status</returns>
        bool Remove(T entity);

        /// <summary>
        /// Remove an entity from this
        /// </summary>
        /// <param name="birthMark">the entity's birthmark to remove</param>
        /// <returns>success status</returns>
        bool Remove(string birthMark);

        /// <summary>
        /// Count the entities in this
        /// </summary>
        /// <returns>the count</returns>
        int Count();
    }
}
