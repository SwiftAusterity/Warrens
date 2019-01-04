using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Data.Players;
using NetMud.DataStructure.Administrative;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace NetMud.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminDataApiController : ApiController
    {
        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        [HttpPost]
        [Route("api/AdminDataApi/ChangeAccountRole/{accountName}/{role}", Name = "AdminAPI_ChangeAccountRole")]
        public string ChangeAccountRole(string accountName, short role)
        {
            RoleManager<IdentityRole> roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new ApplicationDbContext()));
            System.Collections.Generic.List<IdentityRole> validRoles = roleManager.Roles.ToList();

            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());
            Account account = user.GameAccount;
            StaffRank userRole = user.GetStaffRank(User);

            if (userRole != StaffRank.Admin)
            {
                if (string.IsNullOrWhiteSpace(accountName) || account.GlobalIdentityHandle.Equals(accountName) || role >= (short)user.GetStaffRank(User))
                    return "failure";
            }

            ApplicationUser userToModify = UserManager.FindByName(accountName);

            if (userToModify == null)
                return "failure";

            System.Collections.Generic.List<string> rolesToRemove = userToModify.Roles.Select(rol => validRoles.First(vR => vR.Id.Equals(rol.RoleId)).Name).ToList();

            foreach (string currentRole in rolesToRemove)
                UserManager.RemoveFromRole(userToModify.Id, currentRole);

            UserManager.AddToRole(userToModify.Id, ((StaffRank)role).ToString());

            return "success";
        }
    }
}
