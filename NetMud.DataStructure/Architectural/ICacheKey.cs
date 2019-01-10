using System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Architectural
{
    /// <summary>
    /// A cache key
    /// </summary>
    public interface ICacheKey : IComparable<ICacheKey>, IEquatable<ICacheKey>, IEqualityComparer<ICacheKey>
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
}
