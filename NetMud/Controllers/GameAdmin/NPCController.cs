using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Communication.Messaging;
using NetMud.Data.EntityBackingData;
using NetMud.Data.System;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Behaviors.System;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Models.Admin;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers.GameAdmin
{
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

        public ActionResult Index(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            var vModel = new ManageNPCDataViewModel(BackingDataCache.GetAll<INonPlayerCharacter>())
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
        public ActionResult Remove(long removeId = -1, string authorizeRemove = "", long unapproveId = -1, string authorizeUnapprove = "")
        {
            string message = string.Empty;

            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.ToString().Equals(authorizeRemove))
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = BackingDataCache.Get<INonPlayerCharacter>(removeId);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.Remove(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveNPC[" + removeId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                    message = "Error; Removal failed.";
            }
            else if (!string.IsNullOrWhiteSpace(authorizeUnapprove) && unapproveId.ToString().Equals(authorizeUnapprove))
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = BackingDataCache.Get<INonPlayerCharacter>(unapproveId);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.ChangeApprovalStatus(authedUser.GameAccount, authedUser.GetStaffRank(User), ApprovalState.Returned))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - UnapproveNPC[" + unapproveId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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
            var vModel = new AddEditNPCDataViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                ValidRaces = BackingDataCache.GetAll<IRace>()
            };

            return View("~/Views/GameAdmin/NPC/Add.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(string Name, string SurName, string Gender, long raceId)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var newObj = new NonPlayerCharacter
            {
                Name = Name,
                SurName = SurName,
                Gender = Gender
            };
            var race = BackingDataCache.Get<IRace>(raceId);

            if (race != null)
                newObj.RaceData = race;

            if (newObj.Create(authedUser.GameAccount, authedUser.GetStaffRank(User)) == null)
                message = "Error; Creation failed.";
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddNPCData[" + newObj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Creation Successful.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            string message = string.Empty;
            var vModel = new AddEditNPCDataViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                ValidRaces = BackingDataCache.GetAll<IRace>()
            };

            var obj = BackingDataCache.Get<INonPlayerCharacter>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            vModel.DataObject = obj;
            vModel.Name = obj.Name;
            vModel.Gender = obj.Gender;
            vModel.SurName = obj.SurName;

            return View("~/Views/GameAdmin/NPC/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string Name, string SurName, string Gender, long raceId, int id)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var obj = BackingDataCache.Get<NonPlayerCharacter>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            obj.Name = Name;
            obj.SurName = SurName;
            obj.Gender = Gender;
            var race = BackingDataCache.Get<IRace>(raceId);

            if (race != null)
                obj.RaceData = race;

            if (obj.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditNPCData[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
                message = "Error; Edit failed.";

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult AddEditDescriptive(long id, short descriptiveType, string phrase)
        {
            string message = string.Empty;

            var obj = BackingDataCache.Get<INonPlayerCharacter>(id);
            if (obj == null)
            {
                message = "That zone does not exist";
                return RedirectToRoute("ModalErrorOrClose", new { Message = message });
            }

            var vModel = new OccurrenceViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                DataObject = obj
            };

            if (descriptiveType > -1)
            {
                var grammaticalType = (GrammaticalType)descriptiveType;
                vModel.OccurrenceDataObject = obj.Descriptives.FirstOrDefault(occurrence => occurrence.Event.Role == grammaticalType
                                                                                        && occurrence.Event.Phrase.Equals(phrase, StringComparison.InvariantCultureIgnoreCase));
            }

            if (vModel.OccurrenceDataObject != null)
            {
                vModel.LexicaDataObject = vModel.OccurrenceDataObject.Event;
                vModel.Strength = vModel.OccurrenceDataObject.Strength;
                vModel.SensoryType = (short)vModel.OccurrenceDataObject.SensoryType;

                vModel.Role = (short)vModel.LexicaDataObject.Role;
                vModel.Type = (short)vModel.LexicaDataObject.Type;
                vModel.Phrase = vModel.LexicaDataObject.Phrase;
            }

            return View("~/Views/GameAdmin/NPC/Occurrence.cshtml", "_chromelessLayout", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddEditDescriptive(long id, OccurrenceViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var obj = BackingDataCache.Get<INonPlayerCharacter>(id);
            if (obj == null)
            {
                message = "That zone does not exist";
                return RedirectToRoute("ModalErrorOrClose", new { Message = message });
            }

            var grammaticalType = (GrammaticalType)vModel.Role;
            var phraseF = vModel.Phrase;
            var existingOccurrence = obj.Descriptives.FirstOrDefault(occurrence => occurrence.Event.Role == grammaticalType
                                                                                && occurrence.Event.Phrase.Equals(phraseF, StringComparison.InvariantCultureIgnoreCase));

            if (existingOccurrence == null)
                existingOccurrence = new Occurrence();

            existingOccurrence.Strength = vModel.Strength;
            existingOccurrence.SensoryType = (MessagingType)vModel.SensoryType;

            var existingEvent = existingOccurrence.Event;

            if (existingEvent == null)
                existingEvent = new Lexica();

            existingEvent.Role = grammaticalType;
            existingEvent.Phrase = vModel.Phrase;
            existingEvent.Type = (LexicalType)vModel.Type;

            int modifierIndex = 0;
            foreach (var currentPhrase in vModel.ModifierPhrases)
            {
                if (!string.IsNullOrWhiteSpace(currentPhrase))
                {
                    if (vModel.ModifierRoles.Count() <= modifierIndex || vModel.ModifierLexicalTypes.Count() <= modifierIndex)
                        break;

                    var phrase = currentPhrase;
                    var role = (GrammaticalType)vModel.ModifierRoles[modifierIndex];
                    var type = (LexicalType)vModel.ModifierLexicalTypes[modifierIndex];

                    existingEvent.TryModify(new Lexica { Role = role, Type = type, Phrase = phrase });
                }

                modifierIndex++;
            }

            existingOccurrence.Event = existingEvent;

            obj.Descriptives.RemoveWhere(occ => occ.Event.Role == grammaticalType
                                                    && occ.Event.Phrase.Equals(phraseF, StringComparison.InvariantCultureIgnoreCase));
            obj.Descriptives.Add(existingOccurrence);

            if (obj.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - Zone AddEditDescriptive[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
            }
            else
                message = "Error; Edit failed.";

            return RedirectToRoute("ModalErrorOrClose", new { Message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveDescriptive(long id, string authorize)
        {
            string message = string.Empty;

            if (string.IsNullOrWhiteSpace(authorize))
                message = "You must check the proper authorize radio button first.";
            else
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());
                var values = authorize.Split(new string[] { "|||" }, StringSplitOptions.RemoveEmptyEntries);

                if (values.Count() != 2)
                    message = "You must check the proper authorize radio button first.";
                else
                {
                    var type = short.Parse(values[0]);
                    var phrase = values[1];

                    var obj = BackingDataCache.Get<INonPlayerCharacter>(id);

                    if (obj == null)
                        message = "That does not exist";
                    else
                    {
                        var grammaticalType = (GrammaticalType)type;
                        var existingOccurrence = obj.Descriptives.FirstOrDefault(occurrence => occurrence.Event.Role == grammaticalType
                                                                                            && occurrence.Event.Phrase.Equals(phrase, StringComparison.InvariantCultureIgnoreCase));

                        if (existingOccurrence != null)
                        {
                            obj.Descriptives.Remove(existingOccurrence);

                            if (obj.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                            {
                                LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveDescriptive[" + id.ToString() + "|" + type.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                                message = "Delete Successful.";
                            }
                            else
                                message = "Error; Removal failed.";
                        }
                        else
                            message = "That does not exist";
                    }
                }
            }

            return RedirectToRoute("ModalErrorOrClose", new { Message = message });
        }

    }
}