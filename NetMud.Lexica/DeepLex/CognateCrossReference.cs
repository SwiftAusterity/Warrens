using System;

namespace NetMud.Lexica.DeepLex
{
    [Serializable]
    public class CognateCrossReference
    {
        /// <summary>
        /// cognate cross-reference label
        /// </summary>
        public string cxl { get; set; }

        /// <summary>
        /// cognate cross-reference targets
        /// </summary>
        public CognateCrossReferenceTarget[] cxtis { get; set; }
    }
}
