using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Data.Players;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Linguistic;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;

namespace NetMud.Controllers
{
    [Authorize(Roles = "Admin,Builder")]
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

        [HttpGet]
        [Route("api/AdminDataApi/GetDictata/{languageCode}/{wordType}/{term}", Name = "AdminAPI_GetDictata")]
        public JsonResult<string[]> GetDictata(string languageCode, LexicalType wordType, string term)
        {
            IEnumerable<ILexeme> words = ConfigDataCache.GetAll<ILexeme>().Where(dict => dict.GetForm(wordType) != null && dict.Name.Contains(term) && dict.Language.GoogleLanguageCode.Equals(languageCode));

            return Json(words.Select(word => word.Name).ToArray());
        }

        [HttpPost]
        [Route("api/AdminDataApi/ChangeAccountRole/{accountName}/{role}", Name = "AdminAPI_ChangeAccountRole")]
        public string ChangeAccountRole(string accountName, short role)
        {
            RoleManager<IdentityRole> roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new ApplicationDbContext()));
            List<IdentityRole> validRoles = roleManager.Roles.ToList();

            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());
            Account account = user.GameAccount;
            StaffRank userRole = user.GetStaffRank(User);

            if (userRole != StaffRank.Admin)
            {
                if (string.IsNullOrWhiteSpace(accountName) || account.GlobalIdentityHandle.Equals(accountName) || role >= (short)user.GetStaffRank(User))
                {
                    return "failure";
                }
            }

            ApplicationUser userToModify = UserManager.FindByName(accountName);

            if (userToModify == null)
            {
                return "failure";
            }

            List<string> rolesToRemove = userToModify.Roles.Select(rol => validRoles.First(vR => vR.Id.Equals(rol.RoleId)).Name).ToList();

            foreach (string currentRole in rolesToRemove)
            {
                UserManager.RemoveFromRole(userToModify.Id, currentRole);
            }

            UserManager.AddToRole(userToModify.Id, ((StaffRank)role).ToString());

            return "success";
        }
    }
}
