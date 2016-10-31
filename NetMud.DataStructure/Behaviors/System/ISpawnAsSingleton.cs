namespace NetMud.DataStructure.Behaviors.System
{
    /// <summary>
    /// Add this if there should only ever be one of each in the world for each backingData entry. Players, rooms, exits/paths, channels? not sure what else yet
    /// </summary>
    public interface ISpawnAsSingleton : ISingleton
    {
        /// <summary>
        /// Get the entity from the live world or spawn it in
        /// </summary>
        void GetFromWorldOrSpawn();
    }

    /// <summary>
    /// Add this if there should only ever be one of each in the world
    /// </summary>
    public interface ISingleton
    {
    }
}
