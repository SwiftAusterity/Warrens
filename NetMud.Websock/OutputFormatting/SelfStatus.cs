using System;

namespace NetMud.Websock.OutputFormatting
{
    [Serializable]
    public class SelfStatus
    {
        public BodyStatus Body { get; set; }
        public MindStatus Mind { get; set; }
    }
}
