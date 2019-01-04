using NetMud.Authentication;
using NetMud.DataStructure.Administrative;
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

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Given Name", Description = "First name.")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Family Name", Description = "Last Name.")]
        [DataType(DataType.Text)]
        public string SurName { get; set; }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Gender", Description = "Your gender. You can use an existing gender or select free text. Non-approved gender groups will get it/they/them pronouns.")]
        [DataType(DataType.Text)]
        public string Gender { get; set; }

        [UnicodeCharacterValidator(ErrorMessage = "Single characters and single emoticons only.")]
        [Display(Name = "Ascii Character", Description = "The character displayed on the visual map for you.")]
        [DataType(DataType.Text)]
        public string AsciiCharacter { get; set; }

        [Display(Name = "Color", Description = "The hex code of the color of the ascii character.")]
        [DataType(DataType.Text)]
        [UIHint("ColorPicker")]
        public string HexColorCode { get; set; }

        [Display(Name = "Super Vision", Description = "Is your visual range maximized.")]
        [UIHint("Boolean")]
        public bool SuperVision { get; set; }

        [Display(Name = "Chosen Role", Description = "The administrative role.")]
        [UIHint("EnumDropDownList")]
        public StaffRank ChosenRole { get; set; }

        public IEnumerable<StaffRank> ValidRoles { get; set; }
        public IPlayerTemplate CharacterObject { get; set; }
        public IAccount DataObject { get; set; }
    }
}