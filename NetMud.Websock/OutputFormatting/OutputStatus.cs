using System;

namespace NetMud.Websock.OutputFormatting
{
    [Serializable]
    public class OutputStatus
    {
        public string Occurrence { get; set; }
        public SelfStatus Self { get; set; }
        public LocalStatus Local { get; set; }
        public ExtendedStatus Extended { get; set; }
        public EnvironmentStatus Environment { get; set; }
        public string SoundToPlay { get; set; }
    }
}
