using System;

namespace NetMud.Lexica.DeepLex
{
    [Serializable]
    public class Etymology
    {
        public string text { get; set; }

        public string[] et_snote { get; set; }
    }
}
