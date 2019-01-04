using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Data.Action;
using NetMud.Data.Architectural.EntityBase;
using NetMud.Data.Inanimates;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Action;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Inanimate;
using NetMud.Models;
using NetMud.Models.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ActionResult = System.Web.Mvc.ActionResult;

namespace NetMud.Controllers.GameAdmin
{
    [Authorize(Roles = "Admin,Builder")]
    public class InanimateController : Controller
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

        public InanimateController()
        {
        }

        public InanimateController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ActionResult Index(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            ManageInanimateDataViewModel vModel = new ManageInanimateDataViewModel(TemplateCache.GetAll<InanimateTemplate>())
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),

                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View("~/Views/GameAdmin/Inanimate/Index.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"GameAdmin/Inanimate/Remove/{removeId?}/{authorizeRemove?}/{unapproveId?}/{authorizeUnapprove?}")]
        public ActionResult Remove(long removeId = -1, string authorizeRemove = "", long unapproveId = -1, string authorizeUnapprove = "")
        {
            string message = string.Empty;

            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.ToString().Equals(authorizeRemove))
            {
                ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = TemplateCache.Get<IInanimateTemplate>(removeId);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.Remove(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveInanimate[" + removeId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                    message = "Error; Removal failed.";
            }
            else if (!string.IsNullOrWhiteSpace(authorizeUnapprove) && unapproveId.ToString().Equals(authorizeUnapprove))
            {
                ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = TemplateCache.Get<IInanimateTemplate>(unapproveId);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.ChangeApprovalStatus(authedUser.GameAccount, authedUser.GetStaffRank(User), ApprovalState.Unapproved))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - UnapproveInanimate[" + unapproveId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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
        public ActionResult Add(long Template = -1)
        {
            AddEditInanimateDataViewModel vModel = new AddEditInanimateDataViewModel(Template)
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                ValidComponents = TemplateCache.GetAll<IInanimateTemplate>(true),
                DataObject = new InanimateTemplate()
            };

            return View("~/Views/GameAdmin/Inanimate/Add.cshtml", vModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(AddEditInanimateDataViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            InanimateTemplate newObj = new InanimateTemplate
            {
                Name = vModel.DataObject.Name,
                AccumulationCap = vModel.DataObject.AccumulationCap,
                HexColorCode = vModel.DataObject.HexColorCode,
                Description = vModel.DataObject.Description,
                AsciiCharacter = vModel.DataObject.AsciiCharacter,
                RandomDebris = vModel.DataObject.RandomDebris,
                Produces = vModel.DataObject.Produces,
                Qualities = vModel.DataObject.Qualities,
                Components = vModel.DataObject.Components,
                SkillRequirements = vModel.DataObject.SkillRequirements
            };

            if (newObj.Create(authedUser.GameAccount, authedUser.GetStaffRank(User)) == null)
                message = "Error; Creation failed.";
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddInanimateData[" + newObj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Creation Successful.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult Edit(int id, long Template = -1)
        {
            string message = string.Empty;
            AddEditInanimateDataViewModel vModel = new AddEditInanimateDataViewModel(-1)
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                ValidComponents = TemplateCache.GetAll<IInanimateTemplate>(true)
            };

            InanimateTemplate obj = TemplateCache.Get<InanimateTemplate>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            vModel.DataObject = obj;

            //Apply the Actions from this other guy, dont suck in actions from ourself thats stupid
            if (Template > -1 && id != Template)
            {
                bool changedActions = false;
                var DataTemplate = TemplateCache.Get<IInanimateTemplate>(Template);

                List<IDecayEvent> decayEvents = new List<IDecayEvent>();
                foreach (var action in DataTemplate.DecayEvents)
                {
                    decayEvents.Add((IDecayEvent)action.Clone());
                    changedActions = true;
                }
                obj.DecayEvents = new HashSet<IDecayEvent>(decayEvents);

                List<IUse> uses = new List<IUse>();
                foreach (var action in DataTemplate.Uses)
                {
                    uses.Add((IUse)action.Clone());
                    changedActions = true;
                }
                obj.Uses = new HashSet<IUse>(uses);

                List<IInteraction> interactions = new List<IInteraction>();
                foreach (var action in DataTemplate.Interactions)
                {
                    interactions.Add((IInteraction)action.Clone());
                    changedActions = true;
                }
                obj.Interactions = new HashSet<IInteraction>(interactions);

                if (changedActions)
                {
                    if (obj.Save(vModel.authedUser.GameAccount, vModel.authedUser.GetStaffRank(User)))
                    {
                        LoggingUtility.LogAdminCommandUsage("*WEB* - EditInanimateData[" + obj.Id.ToString() + "]", vModel.authedUser.GameAccount.GlobalIdentityHandle);
                        message = "Edit Successful.";
                    }
                    else
                        message = "Error; Edit failed.";
                }
            }

            return View("~/Views/GameAdmin/Inanimate/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, AddEditInanimateDataViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            InanimateTemplate obj = TemplateCache.Get<InanimateTemplate>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            obj.Name = vModel.DataObject.Name;
            obj.AccumulationCap = vModel.DataObject.AccumulationCap;
            obj.HexColorCode = vModel.DataObject.HexColorCode;
            obj.Description = vModel.DataObject.Description;
            obj.AsciiCharacter = vModel.DataObject.AsciiCharacter;
            obj.RandomDebris = vModel.DataObject.RandomDebris;
            obj.Qualities = vModel.DataObject.Qualities;
            obj.Components = vModel.DataObject.Components;
            obj.SkillRequirements = vModel.DataObject.SkillRequirements;

            if (obj.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditInanimateData[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
                message = "Error; Edit failed.";

            return RedirectToAction("Index", new { Message = message });
        }

        #region Actions
        [HttpGet]
        public ActionResult AddInteraction(long id)
        {
            var origin = TemplateCache.Get<IInanimateTemplate>(id);

            if (origin == null)
                return RedirectToAction("Index", new { Message = "Invalid Tile" });

            AddEditInteractionViewModel vModel = new AddEditInteractionViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                ParentObject = origin,
                DataObject = new Interaction(),
                ClassType = "Inanimate"
            };

            return View("AddInteraction", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddInteraction(long id, AddEditInteractionViewModel vModel)
        {
            var origin = TemplateCache.Get<IInanimateTemplate>(id);
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            return RedirectToAction("Edit", new { Message = ActionUtility.AddInteraction(origin, vModel.DataObject, authedUser, User), id });
        }

        [HttpGet]
        public ActionResult EditInteraction(long id, string actionName)
        {
            var origin = TemplateCache.Get<IInanimateTemplate>(id);

            if (origin == null)
                return RedirectToAction("Index", new { Message = "Invalid Item" });

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
                ClassType = "Inanimate"
            };

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditInteraction(long id, string actionName, AddEditInteractionViewModel vModel)
        {
            var origin = TemplateCache.Get<IInanimateTemplate>(id);
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            if (origin == null)
                return RedirectToAction("Index", new { Message = "Invalid Item" });

            var action = origin.Interactions.FirstOrDefault(act => act.Name.Equals(actionName, StringComparison.InvariantCultureIgnoreCase));

            if (action == null)
            {
                return RedirectToAction("Edit", new { Message = "That is an invalid action.", id });
            }

            return RedirectToAction("Edit", new { Message = ActionUtility.EditInteraction(origin, action, vModel.DataObject, authedUser, User), id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveInteraction(long id, string removeId, string authorize)
        {
            string message = string.Empty;

            if (string.IsNullOrWhiteSpace(authorize))
                message = "You must check the proper authorize radio button first.";
            else
            {
                ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

                if (!authorize.Equals(removeId))
                    message = "You must check the proper authorize radio button first.";
                else
                {
                    var origin = TemplateCache.Get<IInanimateTemplate>(id);

                    message = ActionUtility.RemoveInteraction(origin, authorize, authedUser, User);
                }
            }

            return RedirectToAction("Edit", new { Message = message, id });
        }

        [HttpGet]
        public ActionResult AddDecayEvent(long id)
        {
            var origin = TemplateCache.Get<IInanimateTemplate>(id);

            if (origin == null)
                return RedirectToAction("Index", new { Message = "Invalid Tile" });

            AddEditDecayEventViewModel vModel = new AddEditDecayEventViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                DataObject = new DecayEvent(),
                ParentObject = origin,
                ClassType = "Inanimate"
            };

            return View("AddDecayEvent", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddDecayEvent(long id, AddEditDecayEventViewModel vModel)
        {
            var origin = TemplateCache.Get<IInanimateTemplate>(id);
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            return RedirectToAction("Edit", new { Message = ActionUtility.AddDecayEvent(origin, vModel.DataObject, authedUser, User), id });
        }

        [HttpGet]
        public ActionResult EditDecayEvent(long id, string actionName)
        {
            var origin = TemplateCache.Get<IInanimateTemplate>(id);

            if (origin == null)
                return RedirectToAction("Index", new { Message = "Invalid Item" });

            var action = origin.DecayEvents.FirstOrDefault(act => act.Name.Equals(actionName, StringComparison.InvariantCultureIgnoreCase));

            if (action == null)
            {
                return RedirectToAction("Edit", new { Message = "That is an invalid action.", id });
            }

            AddEditDecayEventViewModel vModel = new AddEditDecayEventViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                DataObject = action,
                ParentObject = origin,
                ClassType = "Inanimate"
            };

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditDecayEvent(long id, string actionName, AddEditDecayEventViewModel vModel)
        {
            var origin = TemplateCache.Get<IInanimateTemplate>(id);
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            if (origin == null)
                return RedirectToAction("Index", new { Message = "Invalid Item" });

            var action = origin.DecayEvents.FirstOrDefault(act => act.Name.Equals(actionName, StringComparison.InvariantCultureIgnoreCase));

            if (action == null)
            {
                return RedirectToAction("Edit", new { Message = "That is an invalid action.", id });
            }

            return RedirectToAction("Edit", new { Message = ActionUtility.EditDecayEvent(origin, action, vModel.DataObject, authedUser, User), id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveDecayEvent(long id, string removeId, string authorize)
        {
            string message = string.Empty;

            if (string.IsNullOrWhiteSpace(authorize))
                message = "You must check the proper authorize radio button first.";
            else
            {
                ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

                if (!authorize.Equals(removeId))
                    message = "You must check the proper authorize radio button first.";
                else
                {
                    var origin = TemplateCache.Get<IInanimateTemplate>(id);

                    message = ActionUtility.RemoveDecayEvent(origin, authorize, authedUser, User);
                }
            }

            return RedirectToAction("Edit", new { Message = message, id });
        }

        [HttpGet]
        public ActionResult AddUse(long id)
        {
            var origin = TemplateCache.Get<IInanimateTemplate>(id);

            if (origin == null)
                return RedirectToAction("Index", new { Message = "Invalid Tile" });

            AddEditUseViewModel vModel = new AddEditUseViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                DataObject = new Use(),
                ParentObject = origin,
                ClassType = "Inanimate"
            };

            return View("AddUse", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddUse(long id, AddEditUseViewModel vModel)
        {
            var origin = TemplateCache.Get<IInanimateTemplate>(id);
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            return RedirectToAction("Edit", new { Message = ActionUtility.AddUse(origin, vModel.DataObject, authedUser, User), id });
        }

        [HttpGet]
        public ActionResult EditUse(long id, string actionName)
        {
            var origin = TemplateCache.Get<IInanimateTemplate>(id);

            if (origin == null)
                return RedirectToAction("Index", new { Message = "Invalid Item" });

            var action = origin.Uses.FirstOrDefault(act => act.Name.Equals(actionName, StringComparison.InvariantCultureIgnoreCase));

            if (action == null)
            {
                return RedirectToAction("Edit", new { Message = "That is an invalid action.", id });
            }

            AddEditUseViewModel vModel = new AddEditUseViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                DataObject = action,
                ParentObject = origin,
                ClassType = "Inanimate",
            };

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUse(long id, string actionName, AddEditUseViewModel vModel)
        {
            var origin = TemplateCache.Get<IInanimateTemplate>(id);
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            if (origin == null)
                return RedirectToAction("Index", new { Message = "Invalid Item" });

            var action = origin.Uses.FirstOrDefault(act => act.Name.Equals(actionName, StringComparison.InvariantCultureIgnoreCase));

            if (action == null)
            {
                return RedirectToAction("Edit", new { Message = "That is an invalid action.", id });
            }

            return RedirectToAction("Edit", new { Message = ActionUtility.EditUse(origin, action, vModel.DataObject, authedUser, User), id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveUse(long id, string removeId, string authorize)
        {
            string message = string.Empty;

            if (string.IsNullOrWhiteSpace(authorize))
                message = "You must check the proper authorize radio button first.";
            else
            {
                ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

                if (!authorize.Equals(removeId))
                    message = "You must check the proper authorize radio button first.";
                else
                {
                    var origin = TemplateCache.Get<IInanimateTemplate>(id);

                    message = ActionUtility.RemoveUse(origin, authorize, authedUser, User);
                }
            }

            return RedirectToAction("Edit", new { Message = message, id });
        }
        #endregion
    }
}