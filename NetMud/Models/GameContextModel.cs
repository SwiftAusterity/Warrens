using NetMud.Authentication;
using NetMud.DataStructure.Player;
using System.Collections.Generic;

namespace NetMud.Models
{
    public class GameContextModel : IBaseViewModel
    {
        public ApplicationUser AuthedUser { get; set; }

        public IDictionary<string, string> MusicTracks { get; set; }
        public HashSet<IPlaylist> MusicPlaylists { get; set; }
    }
}