using NetMud.Authentication;
using NetMud.DataStructure.Player;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.PlayerManagement
{
    public class ManagePlaylistsViewModel : PagedDataModel<IPlaylist>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManagePlaylistsViewModel(IEnumerable<IPlaylist> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IPlaylist, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }
    }

    public class AddEditPlaylistViewModel : IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditPlaylistViewModel()
        {
        }

        [Display(Name = "Name", Description = "The name of the playlist.")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [Display(Name = "Song", Description = "A song in the playlist.")]
        [DataType(DataType.Text)]
        public string[] SongList { get; set; }

        public IDictionary<string, string> ValidSongs { get; set; }
        public IPlaylist DataObject { get; set; }
    }
}