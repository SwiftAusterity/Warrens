namespace NetMud.DataStructure.Architectural.EntityBase
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
        long Capacity { get; set; }

        /// <summary>
        /// Will an entity fit inside
        /// </summary>
        /// <param name="entity">the entity you want to cram in</param>
        /// <returns>does it fit (true) or not (false)</returns>
        bool WillItFit(T entity);
    }
}
