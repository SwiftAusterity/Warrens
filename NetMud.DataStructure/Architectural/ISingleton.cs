namespace NetMud.DataStructure.Architectural
{
    /// <summary>
    /// Add this if there should only ever be one of each in the world
    /// </summary>
    public interface ISingleton<T>
    {
        T GetLiveInstance();
    }
}
