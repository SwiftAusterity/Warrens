using System;

namespace NetMud.Websock.OutputFormatting
{
    [Serializable]
    public class EnvironmentStatus
    {
        public string Visibility { get; set; }
        public string Weather { get; set; }
        public string Celestial { get; set; }
        public string TimeOfDay { get; set; }
    }
}
