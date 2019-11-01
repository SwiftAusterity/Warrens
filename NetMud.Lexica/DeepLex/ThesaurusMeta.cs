using System;

namespace NetMud.Lexica.DeepLex
{
    [Serializable]
    public class ThesaurusMeta
    {
        public string id { get; set; }

        public string uuid { get; set; }

        public string sort { get; set; }

        public string src { get; set; }

        public string section { get; set; }

        public string[] stems { get; set; }

        public bool offensive { get; set; }

        public string[] syns { get; set; }

        public string[] ants { get; set; }

        public Target target { get; set; }
    }
}
