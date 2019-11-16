using System;
using System.Collections.Generic;

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
        public List<CognateCrossReferenceTarget> cxtis { get; set; }

        public CognateCrossReference()
        {
            cxtis = new List<CognateCrossReferenceTarget>();
        }
    }
}
