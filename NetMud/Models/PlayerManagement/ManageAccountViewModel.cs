using NetMud.Authentication;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Player;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.PlayerManagement
{
    public class ManageAccountViewModel : IBaseViewModel
    {
        public ApplicationUser AuthedUser { get; set; }

        [Display(Name = "Log Subscriptions", Description = "What log channels you're subscribed to the live streams of.")]
        [DataType(DataType.Text)]
        public IList<string> LogChannels { get; set; }

        [Display(Name = "Global Handle", Description = "The name you are known for through the system. Not used as a character name.")]
        [DataType(DataType.Text)]
        public string GlobalIdentityHandle { get; set; }

        [Display(Name = "Chosen Role", Description = "The administrative role.")]
        [UIHint("EnumDropDownList")]
        public StaffRank ChosenRole { get; set; }

        public IEnumerable<StaffRank> ValidRoles { get; set; }
        public IAccount DataObject { get; set; }
    }
}