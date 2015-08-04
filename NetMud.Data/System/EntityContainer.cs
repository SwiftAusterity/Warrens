using NetMud.DataAccess;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.SupportingClasses;
using System.Collections;
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
        private const string genericCollectionLabel = "*VoidContainer*";
        /// <summary>
        /// What this actually contains, yeah it's a hashtable of hashtables but whatever
        /// </summary>
        private Dictionary<string, HashSet<string>> Birthmarks;

        /// <summary>
        /// What named containers are attached to this
        /// </summary>
        public IEnumerable<IEntityContainerData<T>> NamedContainers { get; set; }

        /// <summary>
        /// New up an empty container
        /// </summary>
        public EntityContainer()
        {
            Birthmarks = new Dictionary<string, HashSet<string>>();
            NamedContainers = Enumerable.Empty<IEntityContainerData<T>>();

            Birthmarks.Add(genericCollectionLabel, new HashSet<string>());
        }

        /// <summary>
        /// New up an empty container with the named containers it should have
        /// </summary>
        public EntityContainer(IEnumerable<IEntityContainerData<T>> namedContainers)
        {
            Birthmarks = new Dictionary<string, HashSet<string>>();
            NamedContainers = namedContainers;

            Birthmarks.Add(genericCollectionLabel, new HashSet<string>());

            foreach(var container in namedContainers)
                Birthmarks.Add(container.Name, new HashSet<string>());
        }

        #region Universal Accessors
        /// <summary>
        /// Restful list of entities contained (it needs to never store its own objects, only cache references)
        /// </summary>
        public IEnumerable<T> EntitiesContained()
        {
            if (Count() > 0)
                return LiveCache.GetMany<T>(Birthmarks.Values.SelectMany(hs => hs));

            return Enumerable.Empty<T>();
        }

        /// <summary>
        /// Add an entity to this
        /// </summary>
        /// <param name="entity">the entity to add</param>
        /// <returns>success status</returns>
        public bool Add(T entity)
        {
            return Birthmarks[genericCollectionLabel].Add(entity.BirthMark);
        }

        /// <summary>
        /// Does this contain the specified entity
        /// </summary>
        /// <param name="entity">the entity in question</param>
        /// <returns>yes it contains it or no it does not</returns>
        public bool Contains(T entity)
        {
            return Birthmarks.Values.Any(hs => hs.Contains(entity.BirthMark));
        }

        /// <summary>
        /// Remove an entity from this
        /// </summary>
        /// <param name="entity">the entity to remove</param>
        /// <returns>success status</returns>
        public bool Remove(T entity)
        {
            return Birthmarks[genericCollectionLabel].Remove(entity.BirthMark);
        }

        /// <summary>
        /// Remove an entity from this
        /// </summary>
        /// <param name="birthMark">the entity's birthmark to remove</param>
        /// <returns>success status</returns>
        public bool Remove(string birthMark)
        {
            return Birthmarks[genericCollectionLabel].Remove(birthMark);
        }

        /// <summary>
        /// Count the entities in this
        /// </summary>
        /// <returns>the count</returns>
        public int Count()
        {
            return Birthmarks.Values.Sum(hs => hs.Count);
        }
        #endregion

        #region Named Containers
        /// <summary>
        /// Restful list of entities contained (it needs to never store its own objects, only cache references)
        /// </summary>
        public IEnumerable<T> EntitiesContained(string namedContainer)
        {
            if (Count(namedContainer) > 0)
                return LiveCache.GetMany<T>(Birthmarks[namedContainer]);

            return Enumerable.Empty<T>();
        }

        /// <summary>
        /// Add an entity to this
        /// </summary>
        /// <param name="entity">the entity to add</param>
        /// <returns>success status</returns>
        public bool Add(T entity, string namedContainer)
        {
            return Birthmarks[namedContainer].Add(entity.BirthMark);
        }

        /// <summary>
        /// Does this contain the specified entity
        /// </summary>
        /// <param name="entity">the entity in question</param>
        /// <returns>yes it contains it or no it does not</returns>
        public bool Contains(T entity, string namedContainer)
        {
            return Birthmarks[namedContainer].Contains(entity.BirthMark);
        }

        /// <summary>
        /// Remove an entity from this
        /// </summary>
        /// <param name="entity">the entity to remove</param>
        /// <returns>success status</returns>
        public bool Remove(T entity, string namedContainer)
        {
            return Birthmarks[namedContainer].Remove(entity.BirthMark);
        }

        /// <summary>
        /// Remove an entity from this
        /// </summary>
        /// <param name="birthMark">the entity's birthmark to remove</param>
        /// <returns>success status</returns>
        public bool Remove(string birthMark, string namedContainer)
        {
            return Birthmarks[namedContainer].Remove(birthMark);
        }

        /// <summary>
        /// Count the entities in this
        /// </summary>
        /// <returns>the count</returns>
        public int Count(string namedContainer)
        {
            return Birthmarks[namedContainer].Count;
        }
        #endregion
    }

    public class EntityContainerData<T> : IEntityContainerData<T> where T : IEntity
    {

        public long CapacityVolume { get; set; }

        public long CapacityWeight { get; set; }

        public string Name { get; set; }

        public bool WillItFit(T entity)
        {
            //-1 volume means infinite
            if (CapacityVolume < 0)
                return true;

            //TODO: Entity dimensions
            return true;
        }
    }
}
