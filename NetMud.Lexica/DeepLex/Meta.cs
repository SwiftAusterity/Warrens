using System;
using System.Collections.Generic;

namespace NetMud.Lexica.DeepLex
{
    [Serializable]
    public class Meta
    {
        public string id { get; set; }

        public string uuid { get; set; }

        public string sort { get; set; }

        public string src { get; set; }

        public string section { get; set; }

        public List<string> stems { get; set; }

        public bool offensive { get; set; }

        public Meta()
        {
            stems = new List<string>();
        }
    }
}
