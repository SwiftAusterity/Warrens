using NetMud.DataStructure.Base.System;
using System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Base.PlayerConfiguration
{
    /// <summary>
    /// The account configuration for a player
    /// </summary>
    public interface IAccountConfig : IConfigData
    {
        /// <summary>
        /// What account owns this config
        /// </summary>
        IAccount Account { get; }

        /// <summary>
        /// Whether or not the person wants the tutorial tooltips on; false = off
        /// </summary>
        bool UITutorialMode { get; set; }

        /// <summary>
        /// Does someone see chatter from the Gossip network?
        /// </summary>
        bool GossipSubscriber { get; set; }

        /// <summary>
        /// The modules to load. Module, quadrant
        /// </summary>
        IEnumerable<Tuple<IUIModule, int>> UIModules { get; set; }

        /// <summary>
        /// Attempt to restore the config from file
        /// </summary>
        /// <returns>False = no file, True = file</returns>
        bool RestoreConfig(IAccount account);
    }
}
