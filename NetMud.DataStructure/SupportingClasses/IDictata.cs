using NetMud.DataStructure.Base.System;
using System.Collections.Generic;

namespace NetMud.DataStructure.SupportingClasses
{
    public interface IDictata : IConfigData
    {
        /// <summary>
        /// The type of word this is in general
        /// </summary>
        LexicalType WordType { get; set; }

        /// <summary>
        /// Things this is the same as mostly
        /// </summary>
        IEnumerable<IDictata> Synonyms { get; set; }

        /// <summary>
        /// Things this is specifically opposite of mostly
        /// </summary>
        IEnumerable<IDictata> Antonyms { get; set; }
    }
}
