namespace NetMud.DataStructure.SupportingClasses
{
    /// <summary>
    /// Framework for storage/retrieval/management of entity containers in backing data
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IEntityContainerData<T>
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
        /// The name of the container; can be string empty without issue
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Will an entity fit inside
        /// </summary>
        /// <param name="entity">the entity you want to cram in</param>
        /// <returns>does it fit (true) or not (false)</returns>
        bool WillItFit(T entity);
    }
}
