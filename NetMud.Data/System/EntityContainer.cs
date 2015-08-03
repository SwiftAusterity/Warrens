using NetMud.DataAccess;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.SupportingClasses;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.Data.System
{
    /// <summary>
    /// Object that handles any and all "this contains entities" for the system
    /// </summary>
    /// <typeparam name="T">the type of entities it can contain</typeparam>
    public class EntityContainer<T> : IEntityContainer<T> where T : IEntity
    {
        /// <summary>
        /// How large is this container
        /// </summary>
        public long CapacityVolume { get; set; }

        /// <summary>
        /// How much weight can it carry before taking damage
        /// </summary>
        public long CapacityWeight { get; set; }

        /// <summary>
        /// What this actually contains
        /// </summary>
        private HashSet<string> Birthmarks;

        /// <summary>
        /// Restful list of entities contained (it needs to never store its own objects, only cache references)
        /// </summary>
        public IEnumerable<T> EntitiesContained 
        { 
            get
            {
                if(Count() > 0)
                    return LiveCache.GetMany<T>(Birthmarks);

                return Enumerable.Empty<T>();
            }
        }

        /// <summary>
        /// New up an empty container
        /// </summary>
        public EntityContainer()
        {
            Birthmarks = new HashSet<string>();
        }

        /// <summary>
        /// Add an entity to this
        /// </summary>
        /// <param name="entity">the entity to add</param>
        /// <returns>success status</returns>
        public bool Add(T entity)
        {
            return Birthmarks.Add(entity.BirthMark);
        }

        /// <summary>
        /// Does this contain the specified entity
        /// </summary>
        /// <param name="entity">the entity in question</param>
        /// <returns>yes it contains it or no it does not</returns>
        public bool Contains(T entity)
        {
            return Birthmarks.Contains(entity.BirthMark);
        }

        /// <summary>
        /// Remove an entity from this
        /// </summary>
        /// <param name="entity">the entity to remove</param>
        /// <returns>success status</returns>
        public bool Remove(T entity)
        {
            return Birthmarks.Remove(entity.BirthMark);
        }

        /// <summary>
        /// Remove an entity from this
        /// </summary>
        /// <param name="birthMark">the entity's birthmark to remove</param>
        /// <returns>success status</returns>
        public bool Remove(string birthMark)
        {
            return Birthmarks.Remove(birthMark);
        }

        /// <summary>
        /// Count the entities in this
        /// </summary>
        /// <returns>the count</returns>
        public int Count()
        {
            return Birthmarks.Count;
        }
    }
}
