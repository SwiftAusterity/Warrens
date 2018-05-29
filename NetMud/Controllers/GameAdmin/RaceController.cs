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
            var vModel = new ManageRaceDataViewModel(BackingDataCache.GetAll<IRace>())
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),

                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

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
            var vModel = new AddEditRaceViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                ValidMaterials = BackingDataCache.GetAll<IMaterial>(),
                ValidObjects = BackingDataCache.GetAll<IInanimateData>(),
                ValidZones = BackingDataCache.GetAll<IZoneData>()
            };

            return View("~/Views/GameAdmin/Race/Add.cshtml", vModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(AddEditRaceViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var newObj = new Race
            {
                Name = vModel.Name
            };

            if (vModel.ArmsID >= 0 && vModel.ArmsAmount > 0)
            {
                var arm = BackingDataCache.Get<InanimateData>(vModel.ArmsID);

                if (arm != null)
                    newObj.Arms = new Tuple<IInanimateData, short>(arm, vModel.ArmsAmount);
            }

            if (vModel.LegsID >= 0 && vModel.LegsAmount > 0)
            {
                var leg = BackingDataCache.Get<IInanimateData>(vModel.LegsID);

                if (leg != null)
                    newObj.Legs = new Tuple<IInanimateData, short>(leg, vModel.LegsAmount);
            }

            if (vModel.TorsoId >= 0)
            {
                var torso = BackingDataCache.Get<IInanimateData>(vModel.TorsoId);

                if (torso != null)
                    newObj.Torso = torso;
            }

            if (vModel.HeadId >= 0)
            {
                var head = BackingDataCache.Get<IInanimateData>(vModel.HeadId);

                if (head != null)
                    newObj.Head = head;
            }

            if (vModel.StartingLocationId >= 0)
            {
                var zone = BackingDataCache.Get<ZoneData>(vModel.StartingLocationId);

                if (zone != null)
                    newObj.StartingLocation = zone;
            }

            if (vModel.RecallLocationId >= 0)
            {
                var zone = BackingDataCache.Get<ZoneData>(vModel.RecallLocationId);

                if (zone != null)
                    newObj.EmergencyLocation = zone;
            }

            if (vModel.BloodId >= 0)
            {
                var blood = BackingDataCache.Get<Material>(vModel.BloodId);

                if (blood != null)
                    newObj.SanguinaryMaterial = blood;
            }

            newObj.VisionRange = new Tuple<short, short>(vModel.VisionRangeLow, vModel.VisionRangeHigh);
            newObj.TemperatureTolerance = new Tuple<short, short>(vModel.TemperatureToleranceLow, vModel.TemperatureToleranceHigh);

            newObj.Breathes = (RespiratoryType)vModel.Breathes;
            newObj.DietaryNeeds = (DietType)vModel.DietaryNeeds;
            newObj.TeethType = (DamageType)vModel.TeethType;
            newObj.HelpText = vModel.HelpBody;

            if (vModel.ExtraPartsId != null)
            {
                int partIndex = 0;
                var bodyBits = new List<Tuple<IInanimateData, short, string>>();
                foreach (var id in vModel.ExtraPartsId)
                {
                    if (id >= 0)
                    {
                        if (vModel.ExtraPartsAmount.Count() <= partIndex || vModel.ExtraPartsName.Count() <= partIndex)
                            break;

                        var currentName = vModel.ExtraPartsName[partIndex];
                        var currentAmount = vModel.ExtraPartsAmount[partIndex];
                        var partObject = BackingDataCache.Get<IInanimateData>(id);

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
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddRaceData[" + newObj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Creation Successful.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            string message = string.Empty;
            var vModel = new AddEditRaceViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                ValidMaterials = BackingDataCache.GetAll<IMaterial>(),
                ValidObjects = BackingDataCache.GetAll<IInanimateData>(),
                ValidZones = BackingDataCache.GetAll<IZoneData>()
            };

            var obj = BackingDataCache.Get<IRace>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            vModel.DataObject = obj;
            vModel.Name = obj.Name;

            if (obj.Arms != null)
            {
                vModel.ArmsAmount = obj.Arms.Item2;
                vModel.ArmsID = obj.Arms.Item1.Id;
            }

            if (obj.Legs != null)
            {
                vModel.LegsAmount = obj.Legs.Item2;
                vModel.LegsID = obj.Legs.Item1.Id;
            }

            if (obj.BodyParts != null)
            {
                vModel.ExtraPartsAmount = obj.BodyParts.Select(bp => bp.Item2).ToArray();
                vModel.ExtraPartsId = obj.BodyParts.Select(bp => bp.Item1.Id).ToArray(); ;
                vModel.ExtraPartsName = obj.BodyParts.Select(bp => bp.Item3).ToArray(); ;
            }

            if (obj.SanguinaryMaterial != null)
            {
                vModel.BloodId = obj.SanguinaryMaterial.Id;
            }

            vModel.Breathes = (short)obj.Breathes;
            vModel.DietaryNeeds = (short)obj.DietaryNeeds;
            vModel.HeadId = obj.Head.Id;

            if(obj.EmergencyLocation != null)
                vModel.RecallLocationId = obj.EmergencyLocation.Id;

            if(obj.StartingLocation != null)
                vModel.StartingLocationId = obj.StartingLocation.Id;

            vModel.TeethType = (short)obj.TeethType;
            vModel.TemperatureToleranceHigh = obj.TemperatureTolerance.Item2;
            vModel.TemperatureToleranceLow = obj.TemperatureTolerance.Item1;
            vModel.TorsoId = obj.Torso.Id;
            vModel.VisionRangeHigh = obj.VisionRange.Item2;
            vModel.VisionRangeLow = obj.VisionRange.Item1;
            vModel.HelpBody = obj.HelpText;


            return View("~/Views/GameAdmin/Race/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, AddEditRaceViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var obj = BackingDataCache.Get<IRace>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            obj.Name = vModel.Name;

            if (vModel.ArmsID > -1 && vModel.ArmsAmount > 0)
            {
                var arm = BackingDataCache.Get<InanimateData>(vModel.ArmsID);

                if (arm != null)
                    obj.Arms = new Tuple<IInanimateData, short>(arm, vModel.ArmsAmount);
            }

            if (vModel.LegsID > -1 && vModel.LegsAmount > 0)
            {
                var leg = BackingDataCache.Get<IInanimateData>(vModel.LegsID);

                if (leg != null)
                    obj.Legs = new Tuple<IInanimateData, short>(leg, vModel.LegsAmount);
            }

            if (vModel.TorsoId > -1)
            {
                var torso = BackingDataCache.Get<IInanimateData>(vModel.TorsoId);

                if (torso != null)
                    obj.Torso = torso;
            }

            if (vModel.HeadId > -1)
            {
                var head = BackingDataCache.Get<IInanimateData>(vModel.HeadId);

                if (head != null)
                    obj.Head = head;
            }

            if (vModel.StartingLocationId >= 0)
            {
                var zone = BackingDataCache.Get<ZoneData>(vModel.StartingLocationId);

                if (zone != null)
                    obj.StartingLocation = zone;
            }

            if (vModel.RecallLocationId >= 0)
            {
                var zone = BackingDataCache.Get<ZoneData>(vModel.RecallLocationId);

                if (zone != null)
                    obj.EmergencyLocation = zone;
            }

            if (vModel.BloodId > -1)
            {
                var blood = BackingDataCache.Get<Material>(vModel.BloodId);

                if (blood != null)
                    obj.SanguinaryMaterial = blood;
            }

            obj.VisionRange = new Tuple<short, short>(vModel.VisionRangeLow, vModel.VisionRangeHigh);
            obj.TemperatureTolerance = new Tuple<short, short>(vModel.TemperatureToleranceLow, vModel.TemperatureToleranceHigh);

            obj.Breathes = (RespiratoryType)vModel.Breathes;
            obj.DietaryNeeds = (DietType)vModel.DietaryNeeds;
            obj.TeethType = (DamageType)vModel.TeethType;
            obj.HelpText = vModel.HelpBody;

            var bodyBits = new List<Tuple<IInanimateData, short, string>>();
            if (vModel.ExtraPartsId != null && vModel.ExtraPartsAmount != null && vModel.ExtraPartsName != null)
            {
                int partIndex = 0;
                foreach (var partId in vModel.ExtraPartsId)
                {
                    if (partId > -1)
                    {
                        if (vModel.ExtraPartsAmount.Count() <= partIndex || vModel.ExtraPartsName.Count() <= partIndex)
                            break;

                        var currentName = vModel.ExtraPartsName[partIndex];
                        var currentAmount = vModel.ExtraPartsAmount[partIndex];
                        var partObject = BackingDataCache.Get<IInanimateData>(partId);

                        if (partObject != null && currentAmount > 0 && !string.IsNullOrWhiteSpace(currentName))
                            bodyBits.Add(new Tuple<IInanimateData, short, string>(partObject, currentAmount, currentName));
                    }

                    partIndex++;
                }

                obj.BodyParts = bodyBits;
            }

            if (obj.Save())
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditRaceData[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
                message = "Error; Edit failed.";

            return RedirectToAction("Index", new { Message = message });
        }
    }
}