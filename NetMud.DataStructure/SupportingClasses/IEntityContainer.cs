using NetMud.DataStructure.Base.System;
using System;
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
        /// What named containers are attached to this
        /// </summary>
        IEnumerable<IEntityContainerData<T>> NamedContainers { get; set; }

        #region Universal accessors
        /// <summary>
        /// List of entities contained (it needs to never store its own objects, only cache references)
        /// </summary>
        IEnumerable<T> EntitiesContained();

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
        bool Remove(ICacheKey cacheKey);

        /// <summary>
        /// Count the entities in this
        /// </summary>
        /// <returns>the count</returns>
        int Count();
        #endregion

        #region Named accessors
        /// <summary>
        /// List of entities contained sent back with which container they are in
        /// </summary>
        /// <returns>entities paired with their container names</returns>
        IEnumerable<Tuple<string, T>> EntitiesContainedByName();

        /// <summary>
        /// List of entities contained (it needs to never store its own objects, only cache references)
        /// </summary>
        IEnumerable<T> EntitiesContained(string containerName);

        /// <summary>
        /// Add an entity to this
        /// </summary>
        /// <param name="entity">the entity to add</param>
        /// <returns>success status</returns>
        bool Add(T entity, string containerName);

        /// <summary>
        /// Does this contain the specified entity
        /// </summary>
        /// <param name="entity">the entity in question</param>
        /// <returns>yes it contains it or no it does not</returns>
        bool Contains(T entity, string containerName);

        /// <summary>
        /// Remove an entity from this
        /// </summary>
        /// <param name="entity">the entity to remove</param>
        /// <returns>success status</returns>
        bool Remove(T entity, string containerName);

        /// <summary>
        /// Remove an entity from this
        /// </summary>
        /// <param name="birthMark">the entity's birthmark to remove</param>
        /// <returns>success status</returns>
        bool Remove(ICacheKey cacheKey, string containerName);

        /// <summary>
        /// Count the entities in this
        /// </summary>
        /// <returns>the count</returns>
        int Count(string containerName);
        #endregion
    }
}
