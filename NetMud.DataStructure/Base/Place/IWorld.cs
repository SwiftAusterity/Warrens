namespace NetMud.DataStructure.Base.Place
{
    /// <summary>
    /// Highest order of "where am I"
    /// </summary>
    public interface IWorld
    {
        /// <summary>
        /// The room array that makes up the world
        /// </summary>
        ILiveMap WorldMap { get; }

        /// <summary>
        /// The dimension's name
        /// </summary>
        string Name { get; }
    }
}
