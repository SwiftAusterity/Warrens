using System;

namespace NetMud.Websock.OutputFormatting
{
    [Serializable]
    public class BodyStatus
    {
        public decimal Health { get; set; }
        public int Stamina { get; set; }
    }
}
