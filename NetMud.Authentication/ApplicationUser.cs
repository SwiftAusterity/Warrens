using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using NetMud.Data.System;
using NetMud.DataStructure.SupportingClasses;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace NetMud.Authentication
{
    /// <summary>
    /// Authenticated user connected to this web application
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Generates a user identity
        /// </summary>
        /// <param name="manager">the user manager used to get the identity</param>
        /// <returns>the identity (async)</returns>
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);

            // Add custom user claims here
            return userIdentity;
        }

        [ForeignKey("GameAccount")]
        public string GlobalIdentityHandle { get; set; }

        /// <summary>
        /// The game account connected to this user identity
        /// </summary>
        [ForeignKey("GlobalIdentityHandle")]
        public virtual Account GameAccount { get; set; }

        /// <summary>
        /// Get the staffrank of this account
        /// </summary>
        /// <returns>the staffrank</returns>
        public StaffRank GetStaffRank(IPrincipal identity)
        {
            var rank = StaffRank.Player;

            if (identity.IsInRole("Admin"))
                rank = StaffRank.Admin;
            else if (identity.IsInRole("Builder"))
                rank = StaffRank.Builder;
            else if (identity.IsInRole("Guest"))
                rank = StaffRank.Guest;

            return rank;
        }
    }
}