using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using NetMud.DataStructure.Base.System;

namespace NetMud.Models
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

        /// <summary>
        /// The game account connected to this user identity
        /// </summary>
        public virtual IAccount GameAccount { get; set; }
    }

    /// <summary>
    /// DB Context for the user manager
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        /// <summary>
        /// New up the db context
        /// </summary>
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        /// <summary>
        /// Get a new db context for the user manager
        /// </summary>
        /// <returns>the db context</returns>
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}