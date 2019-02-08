using NetMud.Authentication;
using NetMud.DataStructure.Player;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.PlayerManagement
{
    public class ManageAcquaintencesViewModel : PagedDataModel<IAcquaintence>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageAcquaintencesViewModel(IEnumerable<IAcquaintence> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IAcquaintence, bool> SearchFilter
        {
            get
            {
                return item => item.PersonHandle.ToLower().Contains(SearchTerms.ToLower());
            }
        }

        internal override Func<IAcquaintence, object> OrderPrimary
        {
            get
            {
                return item => item.IsFriend;
            }
        }


        internal override Func<IAcquaintence, object> OrderSecondary
        {
            get
            {
                return item => item.PersonHandle;
            }
        }

        [Display(Name = "Account Name", Description = "Account (or gossip user) name for the new acquaintence")]
        [DataType(DataType.Text)]
        public string AcquaintenceName { get; set; }

        [Display(Name = "Friend?", Description = "Is this a friend. On = friend, Off = block user")]
        [UIHint("Boolean")]
        public bool IsFriend { get; set; }

        [Display(Name = "Gossip User", Description = "Is this person an external user coming from the InterMUD Gossip Network.")]
        [UIHint("Boolean")]
        public bool GossipSystem { get; set; }

        [Display(Name = "Notifications", Description = "Events you want to be notified about concerning this person.")]
        [DataType(DataType.Text)]
        public string Notifications { get; set; }
    }
}