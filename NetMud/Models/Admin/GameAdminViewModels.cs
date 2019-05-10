using NetMud.Authentication;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Game;
using NetMud.DataStructure.Gossip;
using NetMud.DataStructure.System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;

namespace NetMud.Models.Admin
{
    public class DashboardViewModel : IBaseViewModel
    {
        public ApplicationUser AuthedUser { get; set; }

        public DashboardViewModel()
        {
            Journals = Enumerable.Empty<IJournalEntry>();

            LivePlayers = 0;
        }

        //Backing Data
        public IEnumerable<IJournalEntry> Journals { get; set; }
        public IEnumerable<IGameTemplate> GameTemplates { get; set; }

        //Running Data
        public Dictionary<string, CancellationTokenSource> LiveTaskTokens { get; set; }

        public int LivePlayers { get; set; }
        [Display(Name = "User Creation", Description = "Are new users allowed to register?")]
        [UIHint("Boolean")]
        public bool UserCreationActive { get; set; }

        [Display(Name = "Admins Only", Description = "Are only admins allowed to log in - noone at StaffRank.Player?")]
        [UIHint("Boolean")]
        public bool AdminsOnly { get; set; }

        //Gossip Configuration
        [Display(Name = "Server Active", Description = "Is gossip supposed to be on?")]
        [UIHint("Boolean")]
        public bool GossipActive { get; set; }

        [Display(Name = "Client Id", Description = "The ID for this gossip client.")]
        [DataType(DataType.Text)]
        public string ClientId { get; set; }

        [Display(Name = "Client Secret", Description = "The client secret to share for auth.")]
        [DataType(DataType.Text)]
        public string ClientSecret { get; set; }

        [Display(Name = "Name", Description = "The name this sends to gossip to represent itself.")]
        [DataType(DataType.Text)]
        public string ClientName { get; set; }

        [Display(Name = "Features", Description = "The name this sends to gossip to represent itself.")]
        [UIHint("TagContainer")]
        public HashSet<string> SupportedFeatures { get; set; }

        [Display(Name = "Channels", Description = "The name this sends to gossip to represent itself.")]
        [UIHint("TagContainer")]
        public HashSet<string> SupportedChannels { get; set; }

        [Display(Name = "Retry Loop Maximum", Description = "The maximum retry value. Higher = more retries.")]
        [Range(200, 1000, ErrorMessage = "Must be between 200 and 1000.")]
        [DataType(DataType.Text)]
        public double SuspendMultiplierMaximum { get; set; }

        [Display(Name = "Retry Loop Multiplier", Description = "How much the retry loop monitor escalates each loop. Higher = less retries.")]
        [Range(1.15, 3, ErrorMessage = "Must be between 1.15 and 3.")]
        [DataType(DataType.Text)]
        public double SuspendMultiplier { get; set; }

        [Display(Name = "Backup Name", Description = "Include a name for this backup to make it a permenant archival point.")]
        [DataType(DataType.Text)]
        public string BackupName { get; set; }

        public IGossipConfig GossipConfigDataObject { get; set; }
        public IGlobalConfig ConfigDataObject { get; set; }
    }
}