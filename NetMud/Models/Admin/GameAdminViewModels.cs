using NetMud.Authentication;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Gaia;
using NetMud.DataStructure.Gossip;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.NPC;
using NetMud.DataStructure.System;
using NetMud.DataStructure.Tile;
using NetMud.DataStructure.Zone;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;

namespace NetMud.Models.Admin
{
    public class DashboardViewModel : IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public DashboardViewModel()
        {
            Inanimates = Enumerable.Empty<IInanimateTemplate>();
            TileTypes = Enumerable.Empty<ITileTemplate>();
            NPCs = Enumerable.Empty<INonPlayerCharacterTemplate>();
            Zones = Enumerable.Empty<IZoneTemplate>();
            Worlds = Enumerable.Empty<IGaiaTemplate>();

            HelpFiles = Enumerable.Empty<IHelp>();
            Races = Enumerable.Empty<IRace>();
            Celestials = Enumerable.Empty<ICelestial>();
            Journals = Enumerable.Empty<IJournalEntry>();

            LiveWorlds = 0;
            LiveZones = 0;
            LiveInanimates = 0;
            LiveNPCs = 0;

            LivePlayers = 0;
        }

        //Backing Data
        public IEnumerable<ITileTemplate> TileTypes { get; set; }
        public IEnumerable<IInanimateTemplate> Inanimates { get; set; }
        public IEnumerable<INonPlayerCharacterTemplate> NPCs { get; set; }
        public IEnumerable<IZoneTemplate> Zones { get; set; }
        public IEnumerable<IGaiaTemplate> Worlds { get; set; }

        //Lookup Data
        public IEnumerable<IHelp> HelpFiles { get; set; }
        public IEnumerable<IRace> Races { get; set; }
        public IEnumerable<ICelestial> Celestials { get; set; }
        public IEnumerable<IJournalEntry> Journals { get; set; }

        //Running Data
        public Dictionary<string, CancellationTokenSource> LiveTaskTokens { get; set; }

        public int LiveWorlds { get; set; }
        public int LiveZones { get; set; }
        public int LiveInanimates { get; set; }
        public int LiveNPCs { get; set; }
        public int LivePlayers { get; set; }

        //Global Configuration
        [Display(Name = "Websocket Portal", Description = "Is the websockets portal allowing new connections?")]
        [UIHint("Boolean")]
        public bool WebsocketPortalActive { get; set; }

        [Display(Name = "User Creation", Description = "Are new users allowed to register?")]
        [UIHint("Boolean")]
        public bool UserCreationActive { get; set; }

        [Display(Name = "Admins Only", Description = "Are only admins allowed to log in - noone at StaffRank.Player?")]
        [UIHint("Boolean")]
        public bool AdminsOnly { get; set; }

        [Display(Name = "Death Recall", Description = "The zone you return to on death.")]
        [UIHint("BackingDataDropdown")]
        [Required]
        public long DeathRecallZone { get; set; }

        [Display(Name = "Death Coordinate X", Description = "The coordinates you recall to on death.")]
        [DataType(DataType.Text)]
        [Required]
        public short DeathRecallCoordinateX { get; set; }

        [Display(Name = "Death Coordinate Y", Description = "The coordinates you recall to on death.")]
        [DataType(DataType.Text)]
        [Required]
        public short DeathRecallCoordinateY { get; set; }

        [Display(Name = "Subject", Description = "The subject of the death notice notification sent on death.")]
        [DataType(DataType.Text)]
        [Required]
        public string DeathNoticeSubject { get; set; }

        [Display(Name = "From", Description = "The from field for the death notice.")]
        [DataType(DataType.Text)]
        [Required]
        public string DeathNoticeFrom { get; set; }

        [StringLength(2000, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 20)]
        [DataType("Markdown")]
        [Display(Name = "Body", Description = "The body of the death notice.")]
        [Required]
        public string DeathNoticeBody { get; set; }

        [Display(Name = "Quality", Description = " Should any qualities of the player change on death (like money removal).")]
        [DataType(DataType.Text)]
        public string[] QualityChange { get; set; }

        [Display(Name = "Value", Description = " Should any qualities of the player change on death (like money removal).")]
        [DataType(DataType.Text)]
        public int[] QualityChangeValue { get; set; }

        [Display(Name = "Template", Description = "The zone data to base personal farms on.")]
        [UIHint("BackingDataDropdown")]
        [Required]
        public long Basis { get; set; }

        [Display(Name = "Wild Animal", Description = "Random animals to put in personal farms.")]
        [UIHint("BackingDataDropdown")]
        [Required]
        public long[] WildAnimals { get; set; }

        [Display(Name = "Maximum", Description = "Maximum number of random animal to put in personal farms.")]
        [DataType(DataType.Text)]
        public short[] WildAnimalMaximum { get; set; }

        public IEnumerable<INonPlayerCharacterTemplate> ValidAnimals { get; set; }
        public IEnumerable<IZoneTemplate> ValidZones { get; set; }
        public IGlobalConfig ConfigDataObject { get; set; }

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
        //[UIHint("TagContainer")]
        public string SupportedFeatures { get; set; }

        [Display(Name = "Channels", Description = "The name this sends to gossip to represent itself.")]
        //[UIHint("TagContainer")]
        public string SupportedChannels { get; set; }

        [Display(Name = "Retry Loop Maximum", Description = "The maximum retry value. Higher = more retries.")]
        [Range(200, 1000, ErrorMessage = "Must be between 200 and 1000.")]
        [DataType(DataType.Text)]
        public double SuspendMultiplierMaximum { get; set; }

        [Display(Name = "Retry Loop Multiplier", Description = "How much the retry loop monitor escalates each loop. Higher = less retries.")]
        [Range(1.15, 3, ErrorMessage = "Must be between 1.15 and 3.")]
        [DataType(DataType.Text)]
        public double SuspendMultiplier { get; set; }

        public IGossipConfig GossipConfigDataObject { get; set; }
    }
}