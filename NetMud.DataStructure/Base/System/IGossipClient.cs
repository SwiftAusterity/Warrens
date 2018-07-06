namespace NetMud.DataStructure.Base.System
{
    public interface IGossipClient
    {
        string CacheKey { get; }
        void Launch();
        void Shutdown();
        void SendMessage(string userName, string messageBody, string channel = "gossip");
    }
}
