using System;

namespace NetMud.Lexica.DeepLex
{
    [Serializable]
    public class BiographicalNameWrap
    {
        /// <summary>
        /// first name
        /// </summary>
        public string pname { get; set; }

        /// <summary>
        /// surname
        /// </summary>
        public string sname { get; set; }

        /// <summary>
        /// alternate name
        /// </summary>
        public string altname { get; set; }
    }
}
