using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Data.EntityBackingData;
using NetMud.Data.System;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.World;
using NetMud.DataStructure.Behaviors.System;
using NetMud.Models.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers.GameAdmin
{
    [Authorize(Roles = "Admin,Builder")]
    public class GaiaController : Controller
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

        public GaiaController()
        {
        }

        public GaiaController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ActionResult Index(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            var vModel = new ManageGaiaViewModel(BackingDataCache.GetAll<IGaiaData>())
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),

                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View("~/Views/GameAdmin/Gaia/Index.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"GameAdmin/Gaia/Remove/{removeId?}/{authorizeRemove?}/{unapproveId?}/{authorizeUnapprove?}")]
        public ActionResult Remove(long removeId = -1, string authorizeRemove = "", long unapproveId = -1, string authorizeUnapprove = "")
        {
            string message = string.Empty;

            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.ToString().Equals(authorizeRemove))
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = BackingDataCache.Get<IGaiaData>(removeId);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.Remove(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveGaiaData[" + removeId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                    message = "Error; Removal failed.";
            }
            else if (!string.IsNullOrWhiteSpace(authorizeUnapprove) && unapproveId.ToString().Equals(authorizeUnapprove))
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = BackingDataCache.Get<IGaiaData>(unapproveId);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.ChangeApprovalStatus(authedUser.GameAccount, authedUser.GetStaffRank(User), ApprovalState.Returned))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - UnapproveGaiaData[" + unapproveId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Unapproval Successful.";
                }
                else
                    message = "Error; Unapproval failed.";
            }
            else
                message = "You must check the proper remove or unapprove authorization radio button first.";

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult Add()
        {
            var vModel = new AddEditGaiaViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                ValidCelestials = BackingDataCache.GetAll<ICelestial>(true)
            };

            return View("~/Views/GameAdmin/Gaia/Add.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(AddEditGaiaViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());
            var newObj = new GaiaData
            {
                Name = vModel.Name
            };

            var monthNames = new List<string>();
            if (vModel.MonthNames != null)
            {
                foreach (var tag in vModel.MonthNames.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries))
                    monthNames.Add(tag);
            }

            var chrono = new Chronology
            {
                StartingYear = vModel.StartingYear,
                HoursPerDay = vModel.HoursPerDay,
                DaysPerMonth = vModel.DaysPerMonth,
                Months = monthNames
            };

            newObj.ChronologicalSystem = chrono;

            var bodies = new List<ICelestial>();
            if (vModel.CelestialBodies != null)
            {
                foreach (var id in vModel.CelestialBodies.Where(cId => cId >= 0))
                {
                    var celestial = BackingDataCache.Get<ICelestial>(id);

                    if (celestial != null)
                        bodies.Add(celestial);
                }
            }

            newObj.CelestialBodies = bodies;

            if (newObj.Create(authedUser.GameAccount, authedUser.GetStaffRank(User)) == null)
                message = "Error; Creation failed.";
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddGaiaData[" + newObj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Creation Successful.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult Edit(long id)
        {
            string message = string.Empty;
            var vModel = new AddEditGaiaViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                ValidCelestials = BackingDataCache.GetAll<ICelestial>(true)
            };

            var obj = BackingDataCache.Get<IGaiaData>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            vModel.DataObject = obj;
            vModel.Name = obj.Name;
            vModel.StartingYear = obj.ChronologicalSystem.StartingYear;
            vModel.HoursPerDay = obj.ChronologicalSystem.HoursPerDay;
            vModel.DaysPerMonth = obj.ChronologicalSystem.DaysPerMonth;
            vModel.CelestialBodies = obj.CelestialBodies.Select(cb => cb.Id).ToArray();

            return View("~/Views/GameAdmin/Gaia/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(long id, AddEditGaiaViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var obj = BackingDataCache.Get<IGaiaData>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            try
            {
                obj.Name = vModel.Name;

                var monthNames = new List<string>();
                if (vModel.MonthNames != null)
                {
                    foreach (var tag in vModel.MonthNames.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries))
                        monthNames.Add(tag);
                }

                var chrono = new Chronology
                {
                    StartingYear = vModel.StartingYear,
                    HoursPerDay = vModel.HoursPerDay,
                    DaysPerMonth = vModel.DaysPerMonth,
                    Months = monthNames
                };

                obj.ChronologicalSystem = chrono;

                var bodies = new List<ICelestial>();
                if (vModel.CelestialBodies != null)
                {
                    foreach (var cId in vModel.CelestialBodies.Where(cId => cId >= 0))
                    {
                        var celestial = BackingDataCache.Get<ICelestial>(cId);

                        if (celestial != null)
                            bodies.Add(celestial);
                    }
                }

                obj.CelestialBodies = bodies;

                if (obj.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - EditGaiaData[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Edit Successful.";
                }
                else
                    message = "Error; Edit failed.";
            }
            catch
            {
                message = "Error; Edit failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }
    }
}