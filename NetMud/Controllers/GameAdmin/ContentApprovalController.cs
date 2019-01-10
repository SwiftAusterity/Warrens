using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.Models.Admin;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers.GameAdmin
{
    [Authorize(Roles = "Admin,Builder")]
    public class ContentApprovalController : Controller
    {
        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ContentApprovalController()
        {
        }

        public ContentApprovalController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        [HttpGet]
        public ActionResult Index()
        {
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            IOrderedEnumerable<IKeyedData> newList = TemplateCache.GetAll().Where(item => item.GetType().GetInterfaces().Contains(typeof(INeedApproval))
                                                                && item.GetType().GetInterfaces().Contains(typeof(IKeyedData))
                                                                && !item.SuitableForUse && item.CanIBeApprovedBy(authedUser.GetStaffRank(User), authedUser.GameAccount)).OrderBy(item => item.GetType().Name);

            ManageContentApprovalsViewModel viewModel = new ManageContentApprovalsViewModel(newList);

            return View("~/Views/GameAdmin/ContentApproval/Index.cshtml", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApproveDeny(long? approvalId, string authorizeApproval, long? denialId, string authorizeDenial)
        {
            string message = string.Empty;
            bool approve = true;

            string[] approvalIdSplit = authorizeApproval?.Split(new string[] { "|||" }, StringSplitOptions.RemoveEmptyEntries);
            string[] denialIdSplit = authorizeDenial?.Split(new string[] { "|||" }, StringSplitOptions.RemoveEmptyEntries);

            if ((!string.IsNullOrWhiteSpace(authorizeApproval) && approvalIdSplit.Length > 0 && approvalId.ToString().Equals(approvalIdSplit[0])) ||
                (!string.IsNullOrWhiteSpace(authorizeDenial) && denialIdSplit.Length > 0 && denialId.ToString().Equals(denialIdSplit[0])))
            {
                ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());
                IKeyedData obj = null;

                if (!string.IsNullOrWhiteSpace(authorizeDenial) && denialIdSplit.Length > 0 && denialId.ToString().Equals(denialIdSplit[0]))
                {
                    Type type = Type.GetType(denialIdSplit[1]);

                    approve = false;
                    obj = (IKeyedData)TemplateCache.Get(new TemplateCacheKey(type, denialId.Value));
                }
                else if (!string.IsNullOrWhiteSpace(authorizeApproval) && approvalIdSplit.Length > 0 && approvalId.ToString().Equals(approvalIdSplit[0]))
                {
                    Type type = Type.GetType(approvalIdSplit[1]);

                    obj = (IKeyedData)TemplateCache.Get(new TemplateCacheKey(type, approvalId.Value));
                }

                if (obj == null)
                    message = "That does not exist";
                else
                {
                    if(approve)
                    {
                        if (obj.ChangeApprovalStatus(authedUser.GameAccount, authedUser.GetStaffRank(User), ApprovalState.Approved))
                            message = "Approve Successful.";
                        else
                            message = "Error; Approve failed.";

                        LoggingUtility.LogAdminCommandUsage("*WEB* - Approve Content[" + authorizeApproval + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    }
                    else
                    {
                        if (obj.ChangeApprovalStatus(authedUser.GameAccount, authedUser.GetStaffRank(User), ApprovalState.Unapproved))
                            message = "Deny Successful.";
                        else
                            message = "Error; Deny failed.";

                        LoggingUtility.LogAdminCommandUsage("*WEB* - Deny Content[" + authorizeDenial + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    }
                }
            }
            else
                message = "You must check the proper radio button first.";

            return RedirectToAction("Index", new { Message = message });
        }
    }
}