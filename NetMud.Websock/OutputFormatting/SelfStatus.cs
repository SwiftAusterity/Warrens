using System;

namespace NetMud.Websock.OutputFormatting
{
    [Serializable]
    public class SelfStatus
    {
        public BodyStatus Body { get; set; }
        public string CurrentActivity { get; set; }
        public string Qualities { get; set; }
    }
}
