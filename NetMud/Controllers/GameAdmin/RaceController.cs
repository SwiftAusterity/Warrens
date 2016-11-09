using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Data.EntityBackingData;
using NetMud.Data.LookupData;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Behaviors.Actionable;
using NetMud.DataStructure.Behaviors.Automation;
using NetMud.Models.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers.GameAdmin
{
    public class RaceController : Controller
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

        public RaceController()
        {
        }

        public RaceController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ActionResult Index(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            var vModel = new ManageRaceDataViewModel(BackingDataCache.GetAll<Race>());
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());

            vModel.CurrentPageNumber = CurrentPageNumber;
            vModel.ItemsPerPage = ItemsPerPage;
            vModel.SearchTerms = SearchTerms;

            return View("~/Views/GameAdmin/Race/Index.cshtml", vModel);
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

                var obj = BackingDataCache.Get<Race>(ID);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.Remove())
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveRace[" + ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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
            var vModel = new AddEditRaceViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());
            vModel.ValidMaterials = BackingDataCache.GetAll<Material>();
            vModel.ValidObjects = BackingDataCache.GetAll<InanimateData>();
            vModel.ValidRooms = BackingDataCache.GetAll<RoomData>();

            return View("~/Views/GameAdmin/Race/Add.cshtml", vModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(AddEditRaceViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var newObj = new Race();
            newObj.Name = vModel.NewName;

            if (vModel.NewArmsID > 0 && vModel.NewArmsAmount > 0)
            {
                var arm = BackingDataCache.Get<InanimateData>(vModel.NewArmsID);

                if (arm != null)
                    newObj.Arms = new Tuple<IInanimateData, short>(arm, vModel.NewArmsAmount);
            }

            if (vModel.NewLegsID > 0 && vModel.NewLegsAmount > 0)
            {
                var leg = BackingDataCache.Get<InanimateData>(vModel.NewLegsID);

                if (leg != null)
                    newObj.Legs = new Tuple<IInanimateData, short>(leg, vModel.NewLegsAmount);
            }

            if (vModel.NewTorsoId > 0)
            {
                var torso = BackingDataCache.Get<InanimateData>(vModel.NewTorsoId);

                if (torso != null)
                    newObj.Torso = torso;
            }

            if (vModel.NewHeadId > 0)
            {
                var head = BackingDataCache.Get<InanimateData>(vModel.NewHeadId);

                if (head != null)
                    newObj.Head = head;
            }

            if (vModel.NewStartingLocationId > 0)
            {
                var room = BackingDataCache.Get<RoomData>(vModel.NewStartingLocationId);

                if (room != null)
                    newObj.StartingLocation = room;
            }

            if (vModel.NewRecallLocationId > 0)
            {
                var room = BackingDataCache.Get<RoomData>(vModel.NewRecallLocationId);

                if (room != null)
                    newObj.EmergencyLocation = room;
            }

            if (vModel.NewBloodId > 0)
            {
                var blood = BackingDataCache.Get<Material>(vModel.NewBloodId);

                if (blood != null)
                    newObj.SanguinaryMaterial = blood;
            }

            newObj.VisionRange = new Tuple<short, short>(vModel.NewVisionRangeLow, vModel.NewVisionRangeHigh);
            newObj.TemperatureTolerance = new Tuple<short, short>(vModel.NewTemperatureToleranceLow, vModel.NewTemperatureToleranceHigh);

            newObj.Breathes = (RespiratoryType)vModel.NewBreathes;
            newObj.DietaryNeeds = (DietType)vModel.NewDietaryNeeds;
            newObj.TeethType = (DamageType)vModel.NewTeethType;
            newObj.HelpText = vModel.NewHelpBody;

            if (vModel.NewExtraPartsId != null)
            {
                int partIndex = 0;
                var bodyBits = new List<Tuple<IInanimateData, short, string>>();
                foreach (var id in vModel.NewExtraPartsId)
                {
                    if (id > 0)
                    {
                        if (vModel.NewExtraPartsAmount.Count() <= partIndex || vModel.NewExtraPartsName.Count() <= partIndex)
                            break;

                        var currentName = vModel.NewExtraPartsName[partIndex];
                        var currentAmount = vModel.NewExtraPartsAmount[partIndex];
                        var partObject = BackingDataCache.Get<InanimateData>(id);

                        if (partObject != null && currentAmount > 0 && !string.IsNullOrWhiteSpace(currentName))
                            bodyBits.Add(new Tuple<IInanimateData, short, string>(partObject, currentAmount, currentName));
                    }

                    partIndex++;
                }

                newObj.BodyParts = bodyBits;
            }

            if (newObj.Create() == null)
                message = "Error; Creation failed.";
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddRaceData[" + newObj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Creation Successful.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            string message = string.Empty;
            var vModel = new AddEditRaceViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());
            vModel.ValidMaterials = BackingDataCache.GetAll<IMaterial>();
            vModel.ValidObjects = BackingDataCache.GetAll<IInanimateData>();
            vModel.ValidRooms = BackingDataCache.GetAll<IRoomData>();

            var obj = BackingDataCache.Get<IRace>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            vModel.DataObject = obj;
            vModel.NewName = obj.Name;
            vModel.NewArmsAmount = obj.Arms.Item2;
            vModel.NewArmsID = obj.Arms.Item1.ID;
            vModel.NewBloodId = obj.SanguinaryMaterial.ID;
            vModel.NewBreathes = (short)obj.Breathes;
            vModel.NewDietaryNeeds = (short)obj.DietaryNeeds;
            vModel.NewHeadId = obj.Head.ID;
            vModel.NewLegsAmount = obj.Legs.Item2;
            vModel.NewLegsID = obj.Legs.Item1.ID;
            vModel.NewRecallLocationId = obj.EmergencyLocation.ID;
            vModel.NewStartingLocationId = obj.StartingLocation.ID;
            vModel.NewTeethType = (short)obj.TeethType;
            vModel.NewTemperatureToleranceHigh = obj.TemperatureTolerance.Item2;
            vModel.NewTemperatureToleranceLow = obj.TemperatureTolerance.Item1;
            vModel.NewTorsoId = obj.Torso.ID;
            vModel.NewVisionRangeHigh = obj.VisionRange.Item2;
            vModel.NewVisionRangeLow = obj.VisionRange.Item1;
            vModel.NewHelpBody = obj.HelpText;

            vModel.NewExtraPartsAmount = obj.BodyParts.Select(bp => bp.Item2).ToArray();
            vModel.NewExtraPartsId = obj.BodyParts.Select(bp => bp.Item1.ID).ToArray(); ;
            vModel.NewExtraPartsName = obj.BodyParts.Select(bp => bp.Item3).ToArray(); ;

            return View("~/Views/GameAdmin/Race/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, AddEditRaceViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var obj = BackingDataCache.Get<Race>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            obj.Name = vModel.NewName;

            if (vModel.NewArmsID > 0 && vModel.NewArmsAmount > 0)
            {
                var arm = BackingDataCache.Get<InanimateData>(vModel.NewArmsID);

                if (arm != null)
                    obj.Arms = new Tuple<IInanimateData, short>(arm, vModel.NewArmsAmount);
            }

            if (vModel.NewLegsID > 0 && vModel.NewLegsAmount > 0)
            {
                var leg = BackingDataCache.Get<InanimateData>(vModel.NewLegsID);

                if (leg != null)
                    obj.Legs = new Tuple<IInanimateData, short>(leg, vModel.NewLegsAmount);
            }

            if (vModel.NewTorsoId > 0)
            {
                var torso = BackingDataCache.Get<InanimateData>(vModel.NewTorsoId);

                if (torso != null)
                    obj.Torso = torso;
            }

            if (vModel.NewHeadId > 0)
            {
                var head = BackingDataCache.Get<InanimateData>(vModel.NewHeadId);

                if (head != null)
                    obj.Head = head;
            }

            if (vModel.NewStartingLocationId > 0)
            {
                var room = BackingDataCache.Get<RoomData>(vModel.NewStartingLocationId);

                if (room != null)
                    obj.StartingLocation = room;
            }

            if (vModel.NewRecallLocationId > 0)
            {
                var room = BackingDataCache.Get<RoomData>(vModel.NewRecallLocationId);

                if (room != null)
                    obj.EmergencyLocation = room;
            }

            if (vModel.NewBloodId > 0)
            {
                var blood = BackingDataCache.Get<Material>(vModel.NewBloodId);

                if (blood != null)
                    obj.SanguinaryMaterial = blood;
            }

            obj.VisionRange = new Tuple<short, short>(vModel.NewVisionRangeLow, vModel.NewVisionRangeHigh);
            obj.TemperatureTolerance = new Tuple<short, short>(vModel.NewTemperatureToleranceLow, vModel.NewTemperatureToleranceHigh);

            obj.Breathes = (RespiratoryType)vModel.NewBreathes;
            obj.DietaryNeeds = (DietType)vModel.NewDietaryNeeds;
            obj.TeethType = (DamageType)vModel.NewTeethType;
            obj.HelpText = vModel.NewHelpBody;

            var bodyBits = new List<Tuple<IInanimateData, short, string>>();
            if (vModel.NewExtraPartsId != null)
            {
                int partIndex = 0;
                foreach (var partId in vModel.NewExtraPartsId)
                {
                    if (partId > 0)
                    {
                        if (vModel.NewExtraPartsAmount.Count() <= partIndex || vModel.NewExtraPartsName.Count() <= partIndex)
                            break;

                        var currentName = vModel.NewExtraPartsName[partIndex];
                        var currentAmount = vModel.NewExtraPartsAmount[partIndex];
                        var partObject = BackingDataCache.Get<InanimateData>(partId);

                        if (partObject != null && currentAmount > 0 && !string.IsNullOrWhiteSpace(currentName))
                            bodyBits.Add(new Tuple<IInanimateData, short, string>(partObject, currentAmount, currentName));
                    }

                    partIndex++;
                }

                obj.BodyParts = bodyBits;
            }

            if (obj.Save())
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditRaceData[" + obj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
                message = "Error; Edit failed.";

            return RedirectToAction("Index", new { Message = message });
        }
    }
}