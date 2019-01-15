using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.Zone;
using NetMud.Models.Admin;
using NetMud.Data.Architectural.ActorBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NetMud.Data.Inanimate;

namespace NetMud.Controllers.GameAdmin
{
    [Authorize(Roles = "Admin,Builder")]
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
            var vModel = new ManageRaceDataViewModel(TemplateCache.GetAll<IRace>())
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
        [Route(@"GameAdmin/Race/Remove/{removeId?}/{authorizeRemove?}/{unapproveId?}/{authorizeUnapprove?}")]
        public ActionResult Remove(long removeId = -1, string authorizeRemove = "", long unapproveId = -1, string authorizeUnapprove = "")
        {
            string message = string.Empty;

            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.ToString().Equals(authorizeRemove))
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = TemplateCache.Get<IRace>(removeId);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.Remove(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveRace[" + removeId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                    message = "Error; Removal failed.";
            }
            else if (!string.IsNullOrWhiteSpace(authorizeUnapprove) && unapproveId.ToString().Equals(authorizeUnapprove))
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = TemplateCache.Get<IRace>(unapproveId);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.ChangeApprovalStatus(authedUser.GameAccount, authedUser.GetStaffRank(User), ApprovalState.Returned))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - UnapproveRace[" + unapproveId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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
            var vModel = new AddEditRaceViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                ValidMaterials = TemplateCache.GetAll<IMaterial>(),
                ValidObjects = TemplateCache.GetAll<IInanimateTemplate>(),
                ValidZones = TemplateCache.GetAll<IZoneTemplate>()
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
                var arm = TemplateCache.Get<IInanimateTemplate>(vModel.ArmsID);

                if (arm != null)
                    newObj.Arms = new InanimateComponent(arm, vModel.ArmsAmount);
            }

            if (vModel.LegsID >= 0 && vModel.LegsAmount > 0)
            {
                var leg = TemplateCache.Get<IInanimateTemplate>(vModel.LegsID);

                if (leg != null)
                    newObj.Legs = new InanimateComponent(leg, vModel.LegsAmount);
            }

            if (vModel.TorsoId >= 0)
            {
                var torso = TemplateCache.Get<IInanimateTemplate>(vModel.TorsoId);

                if (torso != null)
                    newObj.Torso = torso;
            }

            if (vModel.HeadId >= 0)
            {
                var head = TemplateCache.Get<IInanimateTemplate>(vModel.HeadId);

                if (head != null)
                    newObj.Head = head;
            }

            if (vModel.StartingLocationId >= 0)
            {
                var zone = TemplateCache.Get<IZoneTemplate>(vModel.StartingLocationId);

                if (zone != null)
                    newObj.StartingLocation = zone;
            }

            if (vModel.RecallLocationId >= 0)
            {
                var zone = TemplateCache.Get<IZoneTemplate>(vModel.RecallLocationId);

                if (zone != null)
                    newObj.EmergencyLocation = zone;
            }

            if (vModel.BloodId >= 0)
            {
                var blood = TemplateCache.Get<IMaterial>(vModel.BloodId);

                if (blood != null)
                    newObj.SanguinaryMaterial = blood;
            }

            newObj.VisionRange = new ValueRange<short>(vModel.VisionRangeLow, vModel.VisionRangeHigh);
            newObj.TemperatureTolerance = new ValueRange<short>(vModel.TemperatureToleranceLow, vModel.TemperatureToleranceHigh);

            newObj.Breathes = (RespiratoryType)vModel.Breathes;
            newObj.DietaryNeeds = (DietType)vModel.DietaryNeeds;
            newObj.TeethType = (DamageType)vModel.TeethType;
            newObj.HelpText = vModel.HelpBody;
            newObj.CollectiveNoun = vModel.CollectiveNoun;

            if (vModel.ExtraPartsId != null)
            {
                int partIndex = 0;
                var bodyBits = new HashSet<Tuple<IInanimateComponent, string>>();
                foreach (var id in vModel.ExtraPartsId)
                {
                    if (id >= 0)
                    {
                        if (vModel.ExtraPartsAmount.Count() <= partIndex || vModel.ExtraPartsName.Count() <= partIndex)
                            break;

                        var currentName = vModel.ExtraPartsName[partIndex];
                        var currentAmount = vModel.ExtraPartsAmount[partIndex];
                        var partObject = TemplateCache.Get<IInanimateTemplate>(id);

                        if (partObject != null && currentAmount > 0 && !string.IsNullOrWhiteSpace(currentName))
                            bodyBits.Add(new Tuple<IInanimateComponent, string>(new InanimateComponent(partObject, currentAmount), currentName));
                    }

                    partIndex++;
                }

                newObj.BodyParts = bodyBits;
            }

            if (newObj.Create(authedUser.GameAccount, authedUser.GetStaffRank(User)) == null)
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
                ValidMaterials = TemplateCache.GetAll<IMaterial>(),
                ValidObjects = TemplateCache.GetAll<IInanimateTemplate>(),
                ValidZones = TemplateCache.GetAll<IZoneTemplate>()
            };

            var obj = TemplateCache.Get<IRace>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            vModel.DataObject = obj;
            vModel.Name = obj.Name;

            if (obj.Arms != null)
            {
                vModel.ArmsAmount = obj.Arms.Amount;
                vModel.ArmsID = obj.Arms.Item.Id;
            }

            if (obj.Legs != null)
            {
                vModel.LegsAmount = obj.Legs.Amount;
                vModel.LegsID = obj.Legs.Item.Id;
            }

            if (obj.BodyParts != null)
            {
                vModel.ExtraPartsAmount = obj.BodyParts.Select(bp => bp.Item1.Amount).ToArray();
                vModel.ExtraPartsId = obj.BodyParts.Select(bp => bp.Item1.Item.Id).ToArray(); ;
                vModel.ExtraPartsName = obj.BodyParts.Select(bp => bp.Item2).ToArray(); ;
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
            vModel.TemperatureToleranceHigh = obj.TemperatureTolerance.High;
            vModel.TemperatureToleranceLow = obj.TemperatureTolerance.Low;
            vModel.TorsoId = obj.Torso.Id;
            vModel.VisionRangeHigh = obj.VisionRange.High;
            vModel.VisionRangeLow = obj.VisionRange.Low;
            vModel.HelpBody = obj.HelpText.Value;
            vModel.CollectiveNoun = obj.CollectiveNoun;

            return View("~/Views/GameAdmin/Race/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, AddEditRaceViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var obj = TemplateCache.Get<IRace>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            obj.Name = vModel.Name;

            if (vModel.ArmsID > -1 && vModel.ArmsAmount > 0)
            {
                var arm = TemplateCache.Get<IInanimateTemplate>(vModel.ArmsID);

                if (arm != null)
                    obj.Arms = new InanimateComponent(arm, vModel.ArmsAmount);
            }

            if (vModel.LegsID > -1 && vModel.LegsAmount > 0)
            {
                var leg = TemplateCache.Get<IInanimateTemplate>(vModel.LegsID);

                if (leg != null)
                    obj.Legs = new InanimateComponent(leg, vModel.LegsAmount);
            }

            if (vModel.TorsoId > -1)
            {
                var torso = TemplateCache.Get<IInanimateTemplate>(vModel.TorsoId);

                if (torso != null)
                    obj.Torso = torso;
            }

            if (vModel.HeadId > -1)
            {
                var head = TemplateCache.Get<IInanimateTemplate>(vModel.HeadId);

                if (head != null)
                    obj.Head = head;
            }

            if (vModel.StartingLocationId >= 0)
            {
                var zone = TemplateCache.Get<IZoneTemplate>(vModel.StartingLocationId);

                if (zone != null)
                    obj.StartingLocation = zone;
            }

            if (vModel.RecallLocationId >= 0)
            {
                var zone = TemplateCache.Get<IZoneTemplate>(vModel.RecallLocationId);

                if (zone != null)
                    obj.EmergencyLocation = zone;
            }

            if (vModel.BloodId > -1)
            {
                var blood = TemplateCache.Get<IMaterial>(vModel.BloodId);

                if (blood != null)
                    obj.SanguinaryMaterial = blood;
            }

            obj.VisionRange = new ValueRange<short>(vModel.VisionRangeLow, vModel.VisionRangeHigh);
            obj.TemperatureTolerance = new ValueRange<short>(vModel.TemperatureToleranceLow, vModel.TemperatureToleranceHigh);

            obj.Breathes = (RespiratoryType)vModel.Breathes;
            obj.DietaryNeeds = (DietType)vModel.DietaryNeeds;
            obj.TeethType = (DamageType)vModel.TeethType;
            obj.HelpText = vModel.HelpBody;
            obj.CollectiveNoun = vModel.CollectiveNoun;

            var bodyBits = new HashSet<Tuple<IInanimateComponent, string>>();
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
                        var partObject = TemplateCache.Get<IInanimateTemplate>(partId);

                        if (partObject != null && currentAmount > 0 && !string.IsNullOrWhiteSpace(currentName))
                            bodyBits.Add(new Tuple<IInanimateComponent, string>(new InanimateComponent(partObject, currentAmount), currentName));
                    }

                    partIndex++;
                }

                obj.BodyParts = bodyBits;
            }

            if (obj.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
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