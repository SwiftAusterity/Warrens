using System;

namespace NetMud.Websock.OutputFormatting
{
    [Serializable]
    public class LocalStatus
    {
        public string LocationDescriptive { get; set; }
        public string[] Populace { get; set; }
    }
}
