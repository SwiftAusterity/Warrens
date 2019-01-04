using NetMud.DataStructure.Architectural;

namespace NetMud.DataStructure.Gossip
{
    /// <summary>
    /// Global settings for the entire system
    /// </summary>
    public interface IGossipConfig : IConfigData, NetMud.Gossip.IConfig
    {
    }
}
