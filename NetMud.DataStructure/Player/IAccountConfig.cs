using NetMud.DataStructure.Architectural;

namespace NetMud.DataStructure.Player
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
        /// Does someone see chatter from the Gossip network?
        /// </summary>
        bool GossipSubscriber { get; set; }

        /// <summary>
        /// Attempt to restore the config from file
        /// </summary>
        /// <returns>False = no file, True = file</returns>
        bool RestoreConfig(IAccount account);
    }
}
