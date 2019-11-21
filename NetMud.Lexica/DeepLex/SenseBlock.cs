using System;
using System.Collections.Generic;

namespace NetMud.Lexica.DeepLex
{
    [Serializable]
    public class SenseBlock
    {
        public List<Sense> sense { get; set; }

        public SenseBlock()
        {
            sense = new List<Sense>();
        }

    }
}
