using NetMud.Authentication;
using NetMud.DataStructure.Player;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.Admin
{
    public class ManageNotificationsViewModel : PagedDataModel<IPlayerMessage>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageNotificationsViewModel(IEnumerable<IPlayerMessage> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IPlayerMessage, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }

        internal override Func<IPlayerMessage, object> OrderPrimary
        {
            get
            {
                return item => item.Read ? 1 : 0;
            }
        }


        internal override Func<IPlayerMessage, object> OrderSecondary
        {
            get
            {
                return item => item.Sent;
            }
        }
    }

    public class AddViewNotificationViewModel : IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddViewNotificationViewModel()
        {
        }

        [Display(Name = "Recipient Account", Description = "The account you are sending this to.")]
        [DataType(DataType.Text)]
        public string RecipientAccount { get; set; }

        [Display(Name = "Recipient", Description = "The character you are sending this to. Can be blank.")]
        [DataType(DataType.Text)]
        public string Recipient { get; set; }

        [StringLength(255, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Subject", Description = "The subject of your message.")]
        [DataType(DataType.Text)]
        public string Subject { get; set; }

        [StringLength(2000, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 10)]
        [Display(Name = "Body", Description = "The body of the message.")]
        [DataType("Markdown")]
        public string Body { get; set; }

        public IPlayerMessage DataObject { get; set; }
    }
}