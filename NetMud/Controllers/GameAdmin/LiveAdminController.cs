using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Gaia;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.NPC;
using NetMud.DataStructure.Zone;
using NetMud.Models.Admin;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers.GameAdmin
{
    [Authorize(Roles = "Admin,Builder")]
    public class LiveAdminController : Controller
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

        public LiveAdminController()
        {
        }

        public LiveAdminController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        #region Indexes
        [HttpGet]
        public ActionResult Worlds(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            LiveWorldsViewModel vModel = new LiveWorldsViewModel(LiveCache.GetAll<IGaia>())
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View(vModel);
        }

        [HttpGet]
        [Route(@"LiveAdmin/World/{birthMark}")]
        public ActionResult World(string birthMark)
        {
            ViewGaiaViewModel vModel = new ViewGaiaViewModel(birthMark)
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId())
            };

            return View(vModel);
        }

        [HttpGet]
        public ActionResult Zones(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            LiveZonesViewModel vModel = new LiveZonesViewModel(LiveCache.GetAll<IZone>())
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View(vModel);
        }

        [HttpGet]
        [Route(@"LiveAdmin/Zone/{birthMark}")]
        public ActionResult Zone(string birthMark)
        {
            ViewZoneViewModel vModel = new ViewZoneViewModel(birthMark)
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId())
            };

            return View(vModel);
        }

        [HttpGet]
        public ActionResult Inanimates(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            LiveInanimatesViewModel vModel = new LiveInanimatesViewModel(LiveCache.GetAll<IInanimate>())
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View(vModel);
        }

        [HttpGet]
        [Route(@"LiveAdmin/Inanimate/{birthMark}")]
        public ActionResult Inanimate(string birthMark)
        {
            ViewInanimateViewModel vModel = new ViewInanimateViewModel(birthMark)
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId())
            };

            return View(vModel);
        }

        [HttpGet]
        public ActionResult NPCs(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            LiveNPCsViewModel vModel = new LiveNPCsViewModel(LiveCache.GetAll<INonPlayerCharacter>())
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View(vModel);
        }

        [HttpGet]
        [Route(@"LiveAdmin/NPC/{birthMark}")]
        public ActionResult NPC(string birthMark)
        {
            ViewIntelligenceViewModel vModel = new ViewIntelligenceViewModel(birthMark)
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId())
            };

            return View(vModel);
        }
        #endregion
    }
}