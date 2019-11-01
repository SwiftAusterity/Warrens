using System;

namespace NetMud.Lexica.DeepLex
{
    [Serializable]
    public class Table
    {
        /// <summary>
        /// ID of the target table
        /// </summary>
        public string tableid { get; set; }

        /// <summary>
        /// text to display in table link
        /// </summary>
        public string displayname { get; set; }
    }
}
