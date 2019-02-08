using NetMud.Authentication;
using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.Player;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.PlayerManagement
{
    public class ManageAccountViewModel : IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        [Display(Name = "Tutorial Mode", Description = "Toggle the Game Client UI Tutorial mode off to remove tip popups permenantly.")]
        public bool UITutorialMode { get; set; }

        [Display(Name = "Gossip Enabled", Description = "Toggle whether or not you see chat coming from the InterMUD Gossip Network.")]
        [UIHint("Boolean")]
        public bool GossipSubscriber { get; set; }

        [Display(Name = "Perma-Mute Sound", Description = "Toggle to mute foley effects in game by default.")]
        [UIHint("Boolean")]
        public bool PermanentlyMuteSound { get; set; }

        [Display(Name = "Perma-Mute Music", Description = "Toggle to mute music in game by default.")]
        [UIHint("Boolean")]
        public bool PermanentlyMuteMusic { get; set; }

        [Display(Name = "Log Subscriptions", Description = "What log channels you're subscribed to the live streams of.")]
        [DataType(DataType.Text)]
        public IList<string> LogChannels { get; set; }

        [Display(Name = "Global Handle", Description = "The name you are known for through the system. Not used as a character name.")]
        [DataType(DataType.Text)]
        public string GlobalIdentityHandle { get; set; }

        [Display(Name = "Game UI Language", Description = "The language the game will output to you while playing.")]
        [UIHint("LanguageList")]
        [LanguageDataBinder]
        public ILanguage UILanguage { get; set; }

        public int UIModuleCount { get; set; }
        public int NotificationCount { get; set; }

        [Display(Name = "Chosen Role", Description = "The administrative role.")]
        [UIHint("EnumDropDownList")]
        public StaffRank ChosenRole { get; set; }

        public IEnumerable<ILanguage> ValidLanguages { get; set; }
        public IEnumerable<StaffRank> ValidRoles { get; set; }
        public IAccount DataObject { get; set; }
    }
}