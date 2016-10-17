using NetMud.DataStructure.Base.System;
namespace NetMud.DataStructure.Base.Place
{
    /// <summary>
    /// Highest order of "where am I"
    /// </summary>
    public interface IWorld : ILiveData
    {
        /// <summary>
        /// The room array that makes up the world
        /// </summary>
        IMap WorldMap { get; }

        /// <summary>
        /// The dimension's name
        /// </summary>
        string Name { get; }
    }
}
