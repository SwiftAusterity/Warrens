using Microsoft.AspNet.Identity.EntityFramework;
using NetMud.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;


namespace NetMud.Models.Admin
{
    public class ManagePlayersViewModel : PagedDataModel<ApplicationUser>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

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

        public IEnumerable<IdentityRole> ValidRoles { get; set; }
    }
}