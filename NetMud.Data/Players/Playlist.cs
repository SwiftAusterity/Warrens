﻿using NetMud.DataStructure.Player;
using System;
using System.Collections.Generic;

namespace NetMud.Data.Players
{
    /// <summary>
    ///  Background music playlist
    /// </summary>
    [Serializable]
    public class Playlist : IPlaylist
    {
        /// <summary>
        /// The unique key used to identify, store and retrieve data
        /// </summary>
        public string UniqueKey => Name;

        /// <summary>
        /// The name of the playlist
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// List of song uris
        /// </summary>
        public HashSet<string> Songs { get; set; }
    }
}
