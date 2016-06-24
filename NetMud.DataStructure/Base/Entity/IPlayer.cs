using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Behaviors.System;
using System;

namespace NetMud.DataStructure.Base.Entity
{
    /// <summary>
    /// Player character + account entity class
    /// </summary>
    public interface IPlayer : IMobile, ISpawnAsSingleton
    {
        /// <summary>
        /// Function used to close the connection
        /// </summary>
        void CloseConnection();
    }
}
