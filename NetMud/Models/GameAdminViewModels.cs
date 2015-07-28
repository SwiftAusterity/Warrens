using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NetMud.Models.GameAdmin
{
    public class DashboardViewModel : BaseViewModel
    {
        public DashboardViewModel()
        {
            Inanimates = Enumerable.Empty<IInanimateData>();
            Rooms = Enumerable.Empty<IRoomData>();
            NPCs = Enumerable.Empty<INonPlayerCharacter>();
            LivePlayers = 0;
        }

        public IEnumerable<IRoomData> Rooms { get; set; }
        public IEnumerable<IInanimateData> Inanimates { get; set; }
        public IEnumerable<INonPlayerCharacter> NPCs { get; set; }

        public int LivePlayers { get; set; }
    }

    public class ManageInanimateDataViewModel : BaseViewModel
    {
        public ManageInanimateDataViewModel()
        {
            Inanimates = Enumerable.Empty<IInanimateData>();
        }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [DataType(DataType.Text)]
        [Display(Name = "Name")]
        public string NewName { get; set; }

        public IEnumerable<IInanimateData> Inanimates { get; set; }
    }

    public class ManageRoomDataViewModel : BaseViewModel
    {
        public ManageRoomDataViewModel()
        {
            Rooms = Enumerable.Empty<IRoomData>();
        }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [DataType(DataType.Text)]
        [Display(Name = "Name")]
        public string NewName { get; set; }

        public IEnumerable<IRoomData> Rooms { get; set; }
    }

    public class ManageNPCDataViewModel : BaseViewModel
    {
        public ManageNPCDataViewModel()
        {
            NPCs = Enumerable.Empty<INonPlayerCharacter>();
        }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [DataType(DataType.Text)]
        [Display(Name = "Given Name")]
        public string NewName { get; set; }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [DataType(DataType.Text)]
        [Display(Name = "Family Name")]
        public string NewSurName { get; set; }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [DataType(DataType.Text)]
        [Display(Name = "Gender")]
        public string NewGender { get; set; }

        public IEnumerable<INonPlayerCharacter> NPCs { get; set; }
    }

    public class ManagePlayersViewModel : BaseViewModel
    {
        public ManagePlayersViewModel()
        {
            Players = Enumerable.Empty<IPlayer>();
        }

        public IEnumerable<IPlayer> Players { get; set; }
    }
}