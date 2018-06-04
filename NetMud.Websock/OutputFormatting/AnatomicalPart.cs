using System;

namespace NetMud.Websock.OutputFormatting
{
    [Serializable]
    public class AnatomicalPart
    {
        public string Name { get; set; }
        public OverallStatus Overall { get; set; }
        public string[] Wounds { get; set; }
    }
}
