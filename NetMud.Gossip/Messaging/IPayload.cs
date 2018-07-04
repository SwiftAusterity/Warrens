using Newtonsoft.Json;

namespace NetMud.Gossip.Messaging
{
    public interface IPayload
    {
        [JsonIgnore]
        string Type { get; }
    }
}
