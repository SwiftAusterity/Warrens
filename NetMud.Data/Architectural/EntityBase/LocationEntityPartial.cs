using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.Architectural.EntityBase
{
    /// <summary>
    /// Partial template for live Location entities
    /// </summary>
    public abstract class LocationEntityPartial : EntityPartial, ILocation
    {
        /// <summary>
        /// The name of the object in the data template
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override string TemplateName
        {
            get
            {
                return Template<ILocationData>()?.Name;
            }
        }

        /// <summary>
        /// The backing data for this entity
        /// </summary>
        public override T Template<T>()
        {
            return (T)TemplateCache.Get(new TemplateCacheKey(typeof(ILocationData), TemplateId));
        }

        /// <summary>
        /// current maximum section
        /// </summary>
        public ulong MaxSection { get; set; }

        #region Container
        /// <summary>
        /// Any mobiles (players, npcs) contained in this
        /// </summary>
        public IEntityContainer<IMobile> MobilesInside { get; set; }

        public int Capacity => 1;

        /// <summary>
        /// Get all of the entities matching a type inside this
        /// </summary>
        /// <typeparam name="T">the type</typeparam>
        /// <returns>the contained entities</returns>
        public IEnumerable<T> GetContents<T>()
        {
            IEnumerable<Type> implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            List<T> contents = new List<T>();

            if (implimentedTypes.Contains(typeof(IMobile)))
            {
                contents.AddRange(MobilesInside.EntitiesContained().Select(ent => (T)ent));
            }

            return contents;
        }

        /// <summary>
        /// Get all of the entities matching a type inside this in a named container
        /// </summary>
        /// <typeparam name="T">the type</typeparam>
        /// <returns>the contained entities</returns>
        /// <param name="containerName">the name of the container</param>
        public IEnumerable<T> GetContents<T>(string containerName)
        {
            IEnumerable<Type> implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            List<T> contents = new List<T>();

            if (implimentedTypes.Contains(typeof(IMobile)))
            {
                contents.AddRange(MobilesInside.EntitiesContained(containerName).Select(ent => (T)ent));
            }

            return contents;
        }

        /// <summary>
        /// Move an entity into this
        /// </summary>
        /// <typeparam name="T">the type of the entity to add</typeparam>
        /// <param name="thing">the entity to add</param>
        /// <returns>errors</returns>
        public string MoveInto<T>(T thing)
        {
            return MoveInto(thing, string.Empty);
        }

        /// <summary>
        /// Move an entity into a named container in this
        /// </summary>
        /// <typeparam name="T">the type of the entity to add</typeparam>
        /// <param name="thing">the entity to add</param>
        /// <param name="containerName">the name of the container</param>
        /// <returns>errors</returns>
        public string MoveInto<T>(T thing, string containerName)
        {
            IEnumerable<Type> implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            if (implimentedTypes.Contains(typeof(IMobile)))
            {
                IMobile obj = (IMobile)thing;

                if (MobilesInside.Contains(obj, containerName))
                {
                    return "That is already in the container";
                }

                MobilesInside.Add(obj, containerName);
                UpsertToLiveWorldCache();

                return string.Empty;
            }

            return "Invalid type to move to container.";
        }

        /// <summary>
        /// Move an entity out of this
        /// </summary>
        /// <typeparam name="T">the type of entity to remove</typeparam>
        /// <param name="thing">the entity</param>
        /// <returns>errors</returns>
        public string MoveFrom<T>(T thing)
        {
            return MoveFrom(thing, string.Empty);
        }

        /// <summary>
        /// Move an entity out of this' named container
        /// </summary>
        /// <typeparam name="T">the type of entity to remove</typeparam>
        /// <param name="thing">the entity</param>
        /// <param name="containerName">the name of the container</param>
        /// <returns>errors</returns>
        public string MoveFrom<T>(T thing, string containerName)
        {
            IEnumerable<Type> implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            if (implimentedTypes.Contains(typeof(IMobile)))
            {
                IMobile obj = (IMobile)thing;

                if (!MobilesInside.Contains(obj, containerName))
                {
                    return "That is not in the container";
                }

                MobilesInside.Remove(obj, containerName);
                UpsertToLiveWorldCache();

                return string.Empty;
            }

            return "Invalid type to move from container.";
        }
        #endregion

        public abstract IGlobalPosition GetContainerAsLocation();
    }
}
