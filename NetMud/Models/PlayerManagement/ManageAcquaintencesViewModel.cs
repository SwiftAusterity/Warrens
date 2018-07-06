using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NetMud.Authentication;
using NetMud.DataStructure.Base.PlayerConfiguration;

namespace NetMud.Models.PlayerManagement
{
    public class ManageAcquaintencesViewModel : PagedDataModel<IAcquaintance>, BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageAcquaintencesViewModel(IEnumerable<IAcquaintance> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IAcquaintance, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }

        [Display(Name = "Account Name", Description = "Account (or gossip user) name for the new acquaintence")]
        [DataType(DataType.Text)]
        public string AcquaintenceName { get; set; }

        [Display(Name = "Friend?", Description = "Is this a friend. On = friend, Off = block user")]
        public bool IsFriend { get; set; }

        [Display(Name = "Gossip User", Description = "Is this person an external user coming from the InterMUD Gossip Network.")]
        public bool GossipSystem { get; set; }


        [Display(Name = "Notifications", Description = "Events you want to be notified about concerning this person.")]
        [DataType(DataType.Text)]
        public string Notifications { get; set; }
    }
}