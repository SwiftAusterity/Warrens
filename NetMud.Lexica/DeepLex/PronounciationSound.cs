using System;

namespace NetMud.Lexica.DeepLex
{
    [Serializable]
    public class PronounciationSound
    {
        /// <summary>
        /// The audio file name
        /// </summary>
        public string Audio { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Ref { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Stat { get; set; }
    }
}
