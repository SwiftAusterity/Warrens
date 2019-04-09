using System;

namespace NetMud.Lexica.DeepLex
{
    [Serializable]
    public class Meta
    {
        public string Id { get; set; }

        public string Uuid { get; set; }

        public string Sort { get; set; }

        public string Src { get; set; }

        public string Section { get; set; }

        public string[] Stems { get; set; }

        public bool Offensive { get; set; }

    }
}
