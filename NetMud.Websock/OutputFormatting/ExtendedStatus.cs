using System;

namespace NetMud.Websock.OutputFormatting
{
    [Serializable]
    public class ExtendedStatus
    {
        public string VisibleMap { get; set; }
        public string[] Horizon { get; set; }
    }
}
