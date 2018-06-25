using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.System;
using NetMud.Models.Admin;
using System.Linq;
using System.Web.Mvc;

namespace NetMud.Controllers.GameAdmin
{
    public class ContentApprovalController : Controller
    {
        // GET: ContentApproval
        public ActionResult Index()
        {
            var newList = BackingDataCache.GetAll().Where(item => item.GetType().GetInterfaces().Contains(typeof(INeedApproval))
                                                                && item.GetType().GetInterfaces().Contains(typeof(IKeyedData))
                                                                && !item.Approved).OrderBy(item => item.GetType().Name);

            var viewModel = new ManageContentApprovalsViewModel(newList);

            return View("~/Views/GameAdmin/ContentApproval/Index.cshtml", viewModel);
        }
    }
}