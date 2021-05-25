using System.Security.Claims;
using System.Threading.Tasks;

namespace NetMud.Authentication
{

    /// <summary>
    /// The user manage that handles authentication events
    /// </summary>
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        /// <summary>
        /// New up an auth manager
        /// </summary>
        /// <param name="userManager">the user manager for the app</param>
        /// <param name="authenticationManager">the actual object that handles the auth calls</param>
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        /// <summary>
        /// Create a user identity
        /// </summary>
        /// <param name="user">the user to create the claims ticket for</param>
        /// <returns>the claims ticket (async)</returns>
        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        /// <summary>
        /// Get a new instance of the auth manager
        /// </summary>
        /// <param name="options">options for identity management</param>
        /// <param name="context">the web context</param>
        /// <returns>the auth manager</returns>
        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }
}
