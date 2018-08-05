using NetMud.DataStructure.Base.PlayerConfiguration;

namespace NetMud.DataStructure.Base.System
{
    public interface IGossipClient
    {
        string CacheKey { get; }
        void Launch();
        void Shutdown();
        void SendMessage(string userName, string messageBody, string channel = "gossip");
        void SendDirectMessage(string userName, string targetGame, string targetPlayer, string messageBody);
        void SendNotification(string userName, AcquaintenceNotifications type);
    }
}
