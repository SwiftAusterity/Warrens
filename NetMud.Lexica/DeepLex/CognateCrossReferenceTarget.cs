using System;

namespace NetMud.Lexica.DeepLex
{
    [Serializable]
    public class CognateCrossReferenceTarget
    {
        /// <summary>
        /// cognate cross-reference label
        /// </summary>
        public string cxl { get; set; }

        /// <summary>
        /// when present, use as cross-reference target ID for immediately preceding cxt
        /// </summary>
        public string cxr { get; set; }

        /// <summary>
        /// provides hyperlink text in all cases, and cross-reference target ID when no immediately following cxr
        /// </summary>
        public string cxt { get; set; }

        /// <summary>
        /// sense number of cross-reference target
        /// </summary>
        public string cxn { get; set; }
    }
}
