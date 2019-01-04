using NetMud.DataStructure.Architectural.EntityBase;
using System;

namespace NetMud.Data.Architectural.EntityBase
{
    /// <summary>
    /// Framework for storage/retrieval/management of entity containers in backing data
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class EntityContainerData<T> : IEntityContainerData<T> where T : IEntity
    {
        /// <summary>
        /// How large is this container
        /// </summary>
        public long Capacity { get; set; }

        /// <summary>
        /// Instansiate this empty
        /// </summary>
        public EntityContainerData()
        {
            Capacity = -1;
        }

        /// <summary>
        /// Instansiate this with parameters
        /// </summary>
        /// <param name="capacityVolume">How large is this container</param>
        /// <param name="capacityWeight">How much weight can it carry before taking damage</param>
        /// <param name="name"> The name of the container; can be string empty without issue</param>
        public EntityContainerData(long capacityVolume, long capacityWeight, string name)
        {
            Capacity = capacityVolume;
        }

        /// <summary>
        /// Will an entity fit inside
        /// </summary>
        /// <param name="entity">the entity you want to cram in</param>
        /// <returns>does it fit (true) or not (false)</returns>
        public bool WillItFit(T entity)
        {
            //-1 volume means infinite
            if (Capacity < 0)
                return true;

            //TODO: Entity dimensions
            return true;
        }
    }
}
