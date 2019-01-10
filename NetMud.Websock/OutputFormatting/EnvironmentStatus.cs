using System;

namespace NetMud.Websock.OutputFormatting
{
    [Serializable]
    public class EnvironmentStatus
    {
        public string Visibility { get; set; }
        public Tuple<string, string, string[]> Weather { get; set; }
        public string Sun { get; set; }
        public string Moon { get; set; }
        public string Temperature { get; set; }
        public string Humidity { get; set; }
        public string TimeOfDay { get; set; }
    }
}
