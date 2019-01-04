using System.Collections.Generic;

namespace NetMud.DataStructure.Player
{ 
    /// <summary>
    /// Background music playlist
    /// </summary>
    public interface IPlaylist
    {
        /// <summary>
        /// The unique key used to identify, store and retrieve data
        /// </summary>
        string UniqueKey { get; }

        /// <summary>
        /// The name of the playlist
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// List of song uris
        /// </summary>
        HashSet<string> Songs { get; set; }
    }
}
