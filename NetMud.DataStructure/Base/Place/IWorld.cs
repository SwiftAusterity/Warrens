using NetMud.DataStructure.Base.System;

namespace NetMud.DataStructure.Base.Place
{
    /// <summary>
    /// Highest order of "where am I"
    /// </summary>
    public interface IWorld : IData
    {
        /// <summary>
        /// The room array that makes up the world
        /// </summary>
        IMap WorldMap { get; }
    }
}
