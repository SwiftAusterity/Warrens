using System;

namespace NetMud.Lexica.DeepLex
{
    [Serializable]
    public class QuotationsSection
    {
        /// <summary>
        /// Text
        /// </summary>
        public string t { get; set; }

        /// <summary>
        /// Author Quotation
        /// </summary>
        public AttributionOfQuote aq { get; set; }
    }
}
