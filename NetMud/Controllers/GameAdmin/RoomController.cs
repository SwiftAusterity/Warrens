using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Data.EntityBackingData;
using NetMud.Data.LookupData;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.Supporting;
using NetMud.Models.Admin;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers.GameAdmin
{
    public class RoomController : Controller
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

        public RoomController()
        {
        }

        public RoomController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ActionResult Index(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            var vModel = new ManageRoomDataViewModel(BackingDataCache.GetAll<IRoomData>());
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());

            vModel.CurrentPageNumber = CurrentPageNumber;
            vModel.ItemsPerPage = ItemsPerPage;
            vModel.SearchTerms = SearchTerms;

            return View("~/Views/GameAdmin/Room/Index.cshtml", vModel);
        }

        [HttpGet]
        public ActionResult Map(long ID)
        {
            var vModel = new RoomMapViewModel();

            vModel.Here = BackingDataCache.Get<IRoomData>(ID);

            return View("~/Views/GameAdmin/Room/Map.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Remove(long ID, string authorize)
        {
            string message = string.Empty;

            if (string.IsNullOrWhiteSpace(authorize) || !ID.ToString().Equals(authorize))
                message = "You must check the proper authorize radio button first.";
            else
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = BackingDataCache.Get<IRoomData>(ID);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.Remove())
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveRoom[" + ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                    message = "Error; Removal failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult Add()
        {
            var vModel = new AddEditRoomDataViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());
            vModel.ValidMaterials = BackingDataCache.GetAll<IMaterial>();
            vModel.ValidZones = BackingDataCache.GetAll<IZone>();

            return View("~/Views/GameAdmin/Room/Add.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(AddEditRoomDataViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var newObj = new RoomData();
            newObj.Name = vModel.NewName;
            newObj.Model = new DimensionalModel(vModel.DimensionalModelHeight, vModel.DimensionalModelLength, vModel.DimensionalModelWidth
                                , vModel.DimensionalModelVacuity, vModel.DimensionalModelCavitation);

            var mediumId = vModel.Medium;
            var medium = BackingDataCache.Get<IMaterial>(mediumId);

            if (medium != null)
            {
                newObj.Medium = medium;

                var zoneId = vModel.Zone;
                var zone = BackingDataCache.Get<IZone>(zoneId);

                if (zone != null)
                {
                    newObj.ZoneAffiliation = zone;

                    if (newObj.Create() == null)
                        message = "Error; Creation failed.";
                    else
                    {
                        LoggingUtility.LogAdminCommandUsage("*WEB* - AddRoomData[" + newObj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                        message = "Creation Successful.";
                    }
                }
                else
                    message = "You must include a valid Zone.";
            }
            else
                message = "You must include a valid Medium material.";

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            string message = string.Empty;
            var vModel = new AddEditRoomDataViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());
            vModel.ValidMaterials = BackingDataCache.GetAll<IMaterial>();
            vModel.ValidZones = BackingDataCache.GetAll<IZone>();

            var obj = BackingDataCache.Get<RoomData>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            vModel.DataObject = obj;
            vModel.NewName = obj.Name;
            vModel.DimensionalModelHeight = obj.Model.Height;
            vModel.DimensionalModelLength = obj.Model.Length;
            vModel.DimensionalModelWidth = obj.Model.Width;

            return View("~/Views/GameAdmin/Room/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AddEditRoomDataViewModel vModel, int id)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var obj = BackingDataCache.Get<RoomData>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            obj.Name = vModel.NewName;
            obj.Model.Height = vModel.DimensionalModelHeight;
            obj.Model.Length = vModel.DimensionalModelLength;
            obj.Model.Width = vModel.DimensionalModelWidth;

            var mediumId = vModel.Medium;
            var medium = BackingDataCache.Get<IMaterial>(mediumId);

            if (medium != null)
            {
                obj.Medium = medium;

                var zoneId = vModel.Zone;
                var zone = BackingDataCache.Get<IZone>(zoneId);

                if (zone != null)
                {
                    obj.ZoneAffiliation = zone;

                    if (obj.Save())
                    {
                        LoggingUtility.LogAdminCommandUsage("*WEB* - EditRoomData[" + obj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                        message = "Edit Successful.";
                    }
                    else
                        message = "Error; Edit failed.";
                }
                else
                    message = "You must include a valid Zone.";
            }
            else
                message = "You must include a valid Medium material.";

            return RedirectToAction("Index", new { Message = message });
        }
    }
}