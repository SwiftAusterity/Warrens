using System;

namespace NetMud.Websock.OutputFormatting
{
    [Serializable]
    public class BodyStatus
    {
        public OverallStatus Overall { get; set; }
        public AnatomicalPart[] Anatomy { get; set; }
    }
}
