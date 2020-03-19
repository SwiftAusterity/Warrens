using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Linguistic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;

namespace NetMud.Controllers
{
    [Authorize]
    public class ClientDataApiController : ApiController
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
        [Route("api/ClientDataApi/ToggleTutorialMode/{language}", Name = "ClientDataAPI_ChangeLanguage")]
        public JsonResult<bool> ChangeUILanguage(string language)
        {
            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());

            ILanguage lang = ConfigDataCache.Get<ILanguage>(new ConfigDataCacheKey(typeof(ILanguage), language, ConfigDataType.Language));

            if (user != null && lang != null)
            {
                user.GameAccount.Config.UILanguage = lang;
                user.GameAccount.Config.Save(user.GameAccount, StaffRank.Admin);
            }

            return Json(true);
        }

        [HttpGet]
        [Route("api/ClientDataApi/GetAccountNames", Name = "ClientDataAPI_GetAccountNames")]
        public JsonResult<string[]> GetAccountNames(string term)
        {
            IQueryable<ApplicationUser> accounts = UserManager.Users;

            return Json(accounts.Where(acct => acct.GlobalIdentityHandle.Contains(term)).Select(acct => acct.GlobalIdentityHandle).ToArray());
        }
    }
}
