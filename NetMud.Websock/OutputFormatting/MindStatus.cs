using System;

namespace NetMud.Websock.OutputFormatting
{
    [Serializable]
    public class MindStatus
    {
        public OverallStatus Overall { get; set; }
        public string[] States { get; set; }
    }
}
