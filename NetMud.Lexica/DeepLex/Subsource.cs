using System;

namespace NetMud.Lexica.DeepLex
{
    [Serializable]
    public class Subsource
    {
        /// <summary>
        /// Source work of quote
        /// </summary>
        public string source { get; set; }

        /// <summary>
        /// publication date of quote
        /// </summary>
        public string aqdate { get; set; }
    }
}
