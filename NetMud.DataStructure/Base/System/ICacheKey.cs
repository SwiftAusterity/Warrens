using NetMud.DataStructure.Base.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.DataStructure.Base.System
{
    /// <summary>
    /// A cache key
    /// </summary>
    public interface ICacheKey
    {
        /// <summary>
        /// The type of cache this is for
        /// </summary>
        CacheType CacheType { get; }

        /// <summary>
        /// Hash key used by the cache system
        /// </summary>
        /// <returns>the key's hash</returns>
        string KeyHash();
    }

    public enum CacheType
    {
        Live,
        BackingData,
        Reference
    }
}
