using NetMud.DataStructure.Player;

namespace NetMud.DataStructure.Gossip
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
