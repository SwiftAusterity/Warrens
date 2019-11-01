using System;

namespace NetMud.Lexica.DeepLex
{
    [Serializable]
    public class AttributionOfQuote
    {
        /// <summary>
        /// Name of author
        /// </summary>
        public string auth { get; set; }

        /// <summary>
        /// Source work of quote
        /// </summary>
        public string source { get; set; }

        /// <summary>
        /// publication date of quote
        /// </summary>
        public string aqdate { get; set; }

        /// <summary>
        /// further detail on quote source (eg, name of larger work in which an essay is found); 
        /// </summary>
        public Subsource subsource { get; set; } 
    }
}
