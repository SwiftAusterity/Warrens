using System;
using System.Collections.Generic;

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

        public List<string> stems { get; set; }

        public bool offensive { get; set; }

        public List<List<string>> syns { get; set; }

        public List<List<string>> ants { get; set; }

        public Target target { get; set; }

        public ThesaurusMeta()
        {
            stems = new List<string>();
            syns = new List<List<string>>();
            ants = new List<List<string>>();
        }
    }
}
