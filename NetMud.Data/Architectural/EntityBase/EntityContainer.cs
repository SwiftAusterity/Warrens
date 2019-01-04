using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.Data.Architectural.EntityBase
{
    /// <summary>
    /// Object that handles any and all "this contains entities" for the system
    /// </summary>
    /// <typeparam name="T">the type of entities it can contain</typeparam>
    public class EntityContainer<T> : IEntityContainer<T> where T : IEntity
    {
        private const string genericCollectionLabel = "*VoidContainer*";
        /// <summary>
        /// What this actually contains, yeah it's a hashtable of hashtables but whatever
        /// </summary>
        private HashSet<LiveCacheKey> Birthmarks { get; set; }

        /// <summary>
        /// New up an empty container
        /// </summary>
        public EntityContainer()
        {
            Birthmarks = new HashSet<LiveCacheKey>();
        }

        #region Universal Accessors
        /// <summary>
        /// Restful list of entities contained (it needs to never store its own objects, only cache references)
        /// </summary>
        public IEnumerable<T> EntitiesContained()
        {
            if (Count() > 0)
                return LiveCache.GetMany<T>(Birthmarks);

            return Enumerable.Empty<T>();
        }

        /// <summary>
        /// Add an entity to this
        /// </summary>
        /// <param name="entity">the entity to add</param>
        /// <returns>success status</returns>
        public bool Add(T entity)
        {
            if (Contains(entity))
                return false;

            return Birthmarks.Add(new LiveCacheKey(entity));
        }

        /// <summary>
        /// Does this contain the specified entity
        /// </summary>
        /// <param name="entity">the entity in question</param>
        /// <returns>yes it contains it or no it does not</returns>
        public bool Contains(T entity)
        {
            LiveCacheKey cacheKey = new LiveCacheKey(entity);

            return Birthmarks.Any(bm => bm.Equals(cacheKey));
        }

        /// <summary>
        /// Remove an entity from this
        /// </summary>
        /// <param name="entity">the entity to remove</param>
        /// <returns>success status</returns>
        public bool Remove(T entity)
        {
            return Remove(new LiveCacheKey(entity));
        }

        /// <summary>
        /// Remove an entity from this
        /// </summary>
        /// <param name="birthMark">the entity's birthmark to remove</param>
        /// <returns>success status</returns>
        public bool Remove(ICacheKey cacheKey)
        {
            LiveCacheKey key = (LiveCacheKey)cacheKey;

            if (!Birthmarks.Any(bm => bm.Equals(key)))
                return false;

            return Birthmarks.RemoveWhere(bm => bm.Equals(key)) > 0;
        }

        /// <summary>
        /// Count the entities in this
        /// </summary>
        /// <returns>the count</returns>
        public int Count()
        {
            return Birthmarks.Count;
        }
        #endregion
    }
}
