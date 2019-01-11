using NetMud.DataStructure.Player;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
        [Display(Name = "Name", Description = "The name of the playlist.")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        /// <summary>
        /// List of song uris
        /// </summary>
        [Display(Name = "Song", Description = "A song in the playlist.")]
        [DataType(DataType.Text)]
        public HashSet<string> Songs { get; set; }
    }
}
