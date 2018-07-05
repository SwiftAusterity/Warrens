using System.ComponentModel.DataAnnotations;
using NetMud.Authentication;
using NetMud.DataStructure.Base.System;

namespace NetMud.Models.PlayerManagement
{
    public class ManageAccountViewModel : BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        [Display(Name = "Tutorial Mode", Description = "Toggle the Game Client UI Tutorial mode off to remove tip popups permenantly.")]
        public bool UITutorialMode { get; set; }

        [Display(Name = "Gossip Enabled", Description = "Toggle whether or not you see chat coming from the InterMUD Gossip Network.")]
        public bool GossipSubscriber { get; set; }      

        [Display(Name = "Log Subscriptions", Description = "What log channels you're subscribed to the live streams of.")]
        [DataType(DataType.Text)]
        public string[] LogChannelSubscriptions { get; set; }

        [Display(Name = "Global Handle", Description = "The name you are known for through the system. Not used as a character name.")]
        [DataType(DataType.Text)]
        public string GlobalIdentityHandle { get; set; }

        public int UIModuleCount { get; set; }
        public IAccount DataObject { get; set; }
    }
}