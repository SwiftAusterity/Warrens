using System;
using System.Collections.Generic;

namespace NetMud.Lexica.DeepLex
{
    [Serializable]
    public class Etymology
    {
        public List<string> et_snote { get; set; }

        public Etymology()
        {
            et_snote = new List<string>();
        }
    }
}
