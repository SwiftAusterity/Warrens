using Newtonsoft.Json;
using System;

namespace NetMud.Gossip.Messaging
{
    [Serializable]
    [JsonObject(Id = "heartbeat")]
    public class Heartbeat
    {
    }
}
