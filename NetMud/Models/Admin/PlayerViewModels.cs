using Microsoft.AspNet.Identity.EntityFramework;
using NetMud.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;


namespace NetMud.Models.Admin
{
    public class ManagePlayersViewModel : PagedDataModel<ApplicationUser>
    {
        public ManagePlayersViewModel(IEnumerable<ApplicationUser> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
            ValidRoles = Enumerable.Empty<IdentityRole>();
        }

        internal override Func<ApplicationUser, bool> SearchFilter
        {
            get
            {
                return item => item.GameAccount.GlobalIdentityHandle.ToLower().Contains(SearchTerms.ToLower());
            }
        }

        internal override Func<ApplicationUser, object> OrderPrimary
        {
            get
            {
                return item => item.GameAccount.GlobalIdentityHandle;
            }
        }


        internal override Func<ApplicationUser, object> OrderSecondary
        {
            get
            {
                return null;
            }
        }

        public IEnumerable<IdentityRole> ValidRoles { get; set; }
    }
}