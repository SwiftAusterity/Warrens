using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Data.Action;
using NetMud.Data.NPCs;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Action;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.NPC;
using NetMud.DataStructure.Tile;
using NetMud.Models;
using NetMud.Models.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers.GameAdmin
{
    [Authorize(Roles = "Admin,Builder")]
    public class NPCController : Controller
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

        public NPCController()
        {
        }

        public NPCController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public System.Web.Mvc.ActionResult Index(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            ManageNPCDataViewModel vModel = new ManageNPCDataViewModel(TemplateCache.GetAll<INonPlayerCharacterTemplate>())
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),

                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View("~/Views/GameAdmin/NPC/Index.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"GameAdmin/NPC/Remove/{removeId?}/{authorizeRemove?}/{unapproveId?}/{authorizeUnapprove?}")]
        public System.Web.Mvc.ActionResult Remove(long removeId = -1, string authorizeRemove = "", long unapproveId = -1, string authorizeUnapprove = "")
        {
            string message = string.Empty;

            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.ToString().Equals(authorizeRemove))
            {
                ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = TemplateCache.Get<INonPlayerCharacterTemplate>(removeId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.Remove(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveNPC[" + removeId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                {
                    message = "Error; Removal failed.";
                }
            }
            else if (!string.IsNullOrWhiteSpace(authorizeUnapprove) && unapproveId.ToString().Equals(authorizeUnapprove))
            {
                ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = TemplateCache.Get<INonPlayerCharacterTemplate>(unapproveId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.ChangeApprovalStatus(authedUser.GameAccount, authedUser.GetStaffRank(User), ApprovalState.Unapproved))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - UnapproveNPC[" + unapproveId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Unapproval Successful.";
                }
                else
                {
                    message = "Error; Unapproval failed.";
                }
            }
            else
            {
                message = "You must check the proper remove or unapprove authorization radio button first.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public System.Web.Mvc.ActionResult Add(long Template = -1)
        {
            AddEditNPCDataViewModel vModel = new AddEditNPCDataViewModel(Template)
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                ValidInanimateDatas = TemplateCache.GetAll<IInanimateTemplate>(),
                ValidNPCDatas = TemplateCache.GetAll<INonPlayerCharacterTemplate>(),
                ValidTileDatas = TemplateCache.GetAll<ITileTemplate>(),
                DataObject = new NonPlayerCharacterTemplate()
            };

            return View("~/Views/GameAdmin/NPC/Add.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public System.Web.Mvc.ActionResult Add(AddEditNPCDataViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            var incomingData = vModel.DataObject;

            NonPlayerCharacterTemplate obj = new NonPlayerCharacterTemplate
            {
                Name = incomingData.Name,
                SurName = incomingData.SurName,
                Gender = incomingData.Gender,
                AsciiCharacter = incomingData.AsciiCharacter,
                Description = incomingData.Description,
                HexColorCode = incomingData.HexColorCode,
                TotalHealth = 100,
                TotalStamina = 100,
                Race = incomingData.Race,
                TeachableAbilities = incomingData.TeachableAbilities,
                TeachableProficencies = incomingData.TeachableProficencies,
                WillPurchase = incomingData.WillPurchase,
                WillSell = incomingData.WillSell,
                InventoryRestock = incomingData.InventoryRestock,
                Personality = incomingData.Personality
            };

            if (obj.Create(authedUser.GameAccount, authedUser.GetStaffRank(User)) == null)
            {
                message = "Error; Creation failed.";
            }
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddNPCData[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Creation Successful.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public System.Web.Mvc.ActionResult Edit(long id, INonPlayerCharacterTemplate Template = null)
        {
            string message = string.Empty;
            AddEditNPCDataViewModel vModel = new AddEditNPCDataViewModel(-1)
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                ValidInanimateDatas = TemplateCache.GetAll<IInanimateTemplate>(),
                ValidNPCDatas = TemplateCache.GetAll<INonPlayerCharacterTemplate>(),
                ValidTileDatas = TemplateCache.GetAll<ITileTemplate>()
            };

            var obj = TemplateCache.Get<NonPlayerCharacterTemplate>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            vModel.DataObject = obj;

            //Apply the Actions from this other guy, dont suck in actions from ourself thats stupid
            if (Template != null && id != Template.Id)
            {
                bool changedActions = false;

                List<IDecayEvent> decayEvents = new List<IDecayEvent>();
                foreach (var action in Template.DecayEvents)
                {
                    decayEvents.Add((IDecayEvent)action.Clone());
                    changedActions = true;
                }
                obj.DecayEvents = new HashSet<IDecayEvent>(decayEvents);

                List<IInteraction> interactions = new List<IInteraction>();
                foreach (var action in Template.Interactions)
                {
                    interactions.Add((IInteraction)action.Clone());
                    changedActions = true;
                }
                obj.Interactions = new HashSet<IInteraction>(interactions);

                List<IUse> uses = new List<IUse>();
                foreach (var action in Template.UsableAbilities)
                {
                    uses.Add((IUse)action.Clone());
                    changedActions = true;
                }
                obj.UsableAbilities = new HashSet<IUse>(uses);

                if (changedActions)
                {
                    if (obj.Save(vModel.authedUser.GameAccount, vModel.authedUser.GetStaffRank(User)))
                    {
                        LoggingUtility.LogAdminCommandUsage("*WEB* - EditInanimateData[" + obj.Id.ToString() + "]", vModel.authedUser.GameAccount.GlobalIdentityHandle);
                        message = "Edit Successful.";
                    }
                    else
                    {
                        message = "Error; Edit failed.";
                    }
                }
            }

            return View("~/Views/GameAdmin/NPC/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public System.Web.Mvc.ActionResult Edit(long id, AddEditNPCDataViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            NonPlayerCharacterTemplate obj = TemplateCache.Get<NonPlayerCharacterTemplate>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            obj = vModel.DataObject;
            obj.Name = vModel.DataObject.Name;
            obj.SurName = vModel.DataObject.SurName;
            obj.Gender = vModel.DataObject.Gender;
            obj.AsciiCharacter = vModel.DataObject.AsciiCharacter;
            obj.Description = vModel.DataObject.Description;
            obj.HexColorCode = vModel.DataObject.HexColorCode;
            obj.Race = vModel.DataObject.Race;

            obj.TeachableAbilities = vModel.DataObject.TeachableAbilities;
            obj.TeachableProficencies = vModel.DataObject.TeachableProficencies;

            obj.WillPurchase = vModel.DataObject.WillPurchase;
            obj.WillSell = vModel.DataObject.WillSell;
            obj.InventoryRestock = vModel.DataObject.InventoryRestock;

            obj.Personality = vModel.DataObject.Personality;

            if (obj.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditNPCData[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
            {
                message = "Error; Edit failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        #region Actions
        [HttpGet]
        public System.Web.Mvc.ActionResult AddInteraction(long id)
        {
            var origin = TemplateCache.Get<INonPlayerCharacterTemplate>(id);

            if (origin == null)
            {
                return RedirectToAction("Index", new { Message = "Invalid Tile" });
            }

            AddEditInteractionViewModel vModel = new AddEditInteractionViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                ParentObject = origin,
                DataObject = new Interaction(),
                ClassType = "NPC"
            };

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public System.Web.Mvc.ActionResult AddInteraction(long id, AddEditInteractionViewModel vModel)
        {
            var origin = TemplateCache.Get<INonPlayerCharacterTemplate>(id);
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            return RedirectToAction("Edit", new { Message = ActionUtility.AddInteraction(origin, vModel.DataObject, authedUser, User), id });
        }

        [HttpGet]
        public System.Web.Mvc.ActionResult EditInteraction(long id, string actionName)
        {
            var origin = TemplateCache.Get<INonPlayerCharacterTemplate>(id);

            if (origin == null)
            {
                return RedirectToAction("Index", new { Message = "Invalid NPC" });
            }

            var action = origin.Interactions.FirstOrDefault(act => act.Name.Equals(actionName, StringComparison.InvariantCultureIgnoreCase));

            if (action == null)
            {
                return RedirectToAction("Edit", new { Message = "That is an invalid action.", id });
            }

            AddEditInteractionViewModel vModel = new AddEditInteractionViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                ParentObject = origin,
                DataObject = action,
                ClassType = "NPC"
            };

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public System.Web.Mvc.ActionResult EditInteraction(long id, string actionName, AddEditInteractionViewModel vModel)
        {
            var origin = TemplateCache.Get<INonPlayerCharacterTemplate>(id);
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            if (origin == null)
            {
                return RedirectToAction("Index", new { Message = "Invalid NPC" });
            }

            var action = origin.Interactions.FirstOrDefault(act => act.Name.Equals(actionName, StringComparison.InvariantCultureIgnoreCase));

            if (action == null)
            {
                return RedirectToAction("Edit", new { Message = "That is an invalid action.", id });
            }

            return RedirectToAction("Edit", new { Message = ActionUtility.EditInteraction(origin, action, vModel.DataObject, authedUser, User), id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public System.Web.Mvc.ActionResult RemoveInteraction(long id, string removeId, string authorize)
        {
            string message = string.Empty;

            if (string.IsNullOrWhiteSpace(authorize))
            {
                message = "You must check the proper authorize radio button first.";
            }
            else
            {
                ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

                if (!authorize.Equals(removeId))
                {
                    message = "You must check the proper authorize radio button first.";
                }
                else
                {
                    var origin = TemplateCache.Get<INonPlayerCharacterTemplate>(id);

                    message = ActionUtility.RemoveInteraction(origin, authorize, authedUser, User);
                }
            }

            return RedirectToAction("Edit", new { Message = message, id });
        }

        [HttpGet]
        public System.Web.Mvc.ActionResult AddUse(long id)
        {
            var origin = TemplateCache.Get<INonPlayerCharacterTemplate>(id);

            if (origin == null)
            {
                return RedirectToAction("Index", new { Message = "Invalid Tile" });
            }

            AddEditUseViewModel vModel = new AddEditUseViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                ParentObject = origin,
                DataObject = new Use(),
                ClassType = "NPC"
            };

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public System.Web.Mvc.ActionResult AddUse(long id, AddEditUseViewModel vModel)
        {
            var origin = TemplateCache.Get<INonPlayerCharacterTemplate>(id);
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            return RedirectToAction("Edit", new { Message = ActionUtility.AddUse(origin, vModel.DataObject, authedUser, User), id });
        }

        [HttpGet]
        public System.Web.Mvc.ActionResult EditUse(long id, string actionName)
        {
            var origin = TemplateCache.Get<INonPlayerCharacterTemplate>(id);

            if (origin == null)
            {
                return RedirectToAction("Index", new { Message = "Invalid NPC" });
            }

            var action = origin.UsableAbilities.FirstOrDefault(act => act.Name.Equals(actionName, StringComparison.InvariantCultureIgnoreCase));

            if (action == null)
            {
                return RedirectToAction("Edit", new { Message = "That is an invalid action.", id });
            }

            AddEditUseViewModel vModel = new AddEditUseViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                ParentObject = origin,
                DataObject = action,
                ClassType = "NPC"
            };

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public System.Web.Mvc.ActionResult EditUse(long id, string actionName, AddEditUseViewModel vModel)
        {
            var origin = TemplateCache.Get<INonPlayerCharacterTemplate>(id);
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            if (origin == null)
            {
                return RedirectToAction("Index", new { Message = "Invalid NPC" });
            }

            var action = origin.UsableAbilities.FirstOrDefault(act => act.Name.Equals(actionName, StringComparison.InvariantCultureIgnoreCase));

            if (action == null)
            {
                return RedirectToAction("Edit", new { Message = "That is an invalid action.", id });
            }

            return RedirectToAction("Edit", new { Message = ActionUtility.EditUse(origin, action, vModel.DataObject, authedUser, User), id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public System.Web.Mvc.ActionResult RemoveUse(long id, string removeId, string authorize)
        {
            string message = string.Empty;

            if (string.IsNullOrWhiteSpace(authorize))
            {
                message = "You must check the proper authorize radio button first.";
            }
            else
            {
                ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

                if (!authorize.Equals(removeId))
                {
                    message = "You must check the proper authorize radio button first.";
                }
                else
                {
                    var origin = TemplateCache.Get<INonPlayerCharacterTemplate>(id);

                    message = ActionUtility.RemoveUse(origin, authorize, authedUser, User);
                }
            }

            return RedirectToAction("Edit", new { Message = message, id });
        }


        [HttpGet]
        public System.Web.Mvc.ActionResult AddDecayEvent(long id)
        {
            var origin = TemplateCache.Get<INonPlayerCharacterTemplate>(id);

            if (origin == null)
            {
                return RedirectToAction("Index", new { Message = "Invalid Tile" });
            }

            AddEditDecayEventViewModel vModel = new AddEditDecayEventViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                ParentObject = origin,
                DataObject = new DecayEvent(),
                ClassType = "NPC"
            };

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public System.Web.Mvc.ActionResult AddDecayEvent(long id, AddEditDecayEventViewModel vModel)
        {
            var origin = TemplateCache.Get<INonPlayerCharacterTemplate>(id);
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            return RedirectToAction("Edit", new { Message = ActionUtility.AddDecayEvent(origin, vModel.DataObject, authedUser, User), id });
        }

        [HttpGet]
        public System.Web.Mvc.ActionResult EditDecayEvent(long id, string actionName)
        {
            var origin = TemplateCache.Get<INonPlayerCharacterTemplate>(id);

            if (origin == null)
            {
                return RedirectToAction("Index", new { Message = "Invalid NPC" });
            }

            var action = origin.DecayEvents.FirstOrDefault(act => act.Name.Equals(actionName, StringComparison.InvariantCultureIgnoreCase));

            if (action == null)
            {
                return RedirectToAction("Edit", new { Message = "That is an invalid action.", id });
            }

            AddEditDecayEventViewModel vModel = new AddEditDecayEventViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                ParentObject = origin,
                DataObject = action,
                ClassType = "NPC"
            };

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public System.Web.Mvc.ActionResult EditDecayEvent(long id, string actionName, AddEditDecayEventViewModel vModel)
        {
            var origin = TemplateCache.Get<INonPlayerCharacterTemplate>(id);
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            if (origin == null)
            {
                return RedirectToAction("Index", new { Message = "Invalid NPC" });
            }

            var action = origin.DecayEvents.FirstOrDefault(act => act.Name.Equals(actionName, StringComparison.InvariantCultureIgnoreCase));

            if (action == null)
            {
                return RedirectToAction("Edit", new { Message = "That is an invalid action.", id });
            }

            return RedirectToAction("Edit", new { Message = ActionUtility.EditDecayEvent(origin, action, vModel.DataObject, authedUser, User), id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public System.Web.Mvc.ActionResult RemoveDecayEvent(long id, string removeId, string authorize)
        {
            string message = string.Empty;

            if (string.IsNullOrWhiteSpace(authorize))
            {
                message = "You must check the proper authorize radio button first.";
            }
            else
            {
                ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

                if (!authorize.Equals(removeId))
                {
                    message = "You must check the proper authorize radio button first.";
                }
                else
                {
                    var origin = TemplateCache.Get<INonPlayerCharacterTemplate>(id);

                    message = ActionUtility.RemoveDecayEvent(origin, authorize, authedUser, User);
                }
            }

            return RedirectToAction("Edit", new { Message = message, id });
        }
        #endregion
    }
}