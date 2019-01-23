using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Communication.Lexical;
using NetMud.Data.Linguistic;
using NetMud.Data.Room;
using NetMud.Data.Zone;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Gaia;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.Locale;
using NetMud.DataStructure.Room;
using NetMud.DataStructure.Zone;
using NetMud.Models.Admin;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers.GameAdmin
{
    [Authorize(Roles = "Admin,Builder")]
    public class ZoneController : Controller
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

        public ZoneController()
        {
        }

        public ZoneController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ActionResult Index(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            var vModel = new ManageZoneTemplateViewModel(TemplateCache.GetAll<IZoneTemplate>())
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),

                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View("~/Views/GameAdmin/Zone/Index.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"GameAdmin/Zone/Remove/{removeId?}/{authorizeRemove?}/{unapproveId?}/{authorizeUnapprove?}")]
        public ActionResult Remove(long removeId = -1, string authorizeRemove = "", long unapproveId = -1, string authorizeUnapprove = "")
        {
            string message = string.Empty;

            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.ToString().Equals(authorizeRemove))
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = TemplateCache.Get<IZoneTemplate>(removeId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.Remove(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveZone[" + removeId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                {
                    message = "Error; Removal failed.";
                }
            }
            else if (!string.IsNullOrWhiteSpace(authorizeUnapprove) && unapproveId.ToString().Equals(authorizeUnapprove))
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = TemplateCache.Get<IZoneTemplate>(unapproveId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.ChangeApprovalStatus(authedUser.GameAccount, authedUser.GetStaffRank(User), ApprovalState.Returned))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - UnapproveZone[" + unapproveId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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
        public ActionResult Add()
        {
            var vModel = new AddEditZoneTemplateViewModel()
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                ValidWorlds = TemplateCache.GetAll<IGaiaTemplate>(true),
                DataObject = new ZoneTemplate()
            };

            return View("~/Views/GameAdmin/Zone/Add.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(AddEditZoneTemplateViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var newObj = new ZoneTemplate
            {
                Name = vModel.Name,
                BaseElevation = vModel.BaseElevation,
                PressureCoefficient = vModel.PressureCoefficient,
                TemperatureCoefficient = vModel.TemperatureCoefficient,
                Hemisphere = (HemispherePlacement)vModel.Hemisphere
            };

            var world = TemplateCache.Get<IGaiaTemplate>(vModel.World);

            if (world == null)
            {
                message = "Error; You must choose a valid world.";
            }
            else
            {
                newObj.World = world;

                if (newObj.Create(authedUser.GameAccount, authedUser.GetStaffRank(User)) == null)
                {
                    message = "Error; Creation failed.";
                }
                else
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - AddZone[" + newObj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Creation Successful.";
                }
            }
            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult Edit(long id)
        {
            string message = string.Empty;

            IZoneTemplate obj = TemplateCache.Get<IZoneTemplate>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            var locales = TemplateCache.GetAll<ILocaleTemplate>().Where(locale => locale.ParentLocation.Equals(obj));

            var vModel = new AddEditZoneTemplateViewModel(locales)
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),

                DataObject = obj,
                Name = obj.Name,
                BaseElevation = obj.BaseElevation,
                PressureCoefficient = obj.PressureCoefficient,
                TemperatureCoefficient = obj.TemperatureCoefficient,
                ValidWorlds = TemplateCache.GetAll<IGaiaTemplate>(true),
                World = obj.World == null ? -1 : obj.World.Id,
                Hemisphere = (short)obj.Hemisphere
            };

            return View("~/Views/GameAdmin/Zone/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AddEditZoneTemplateViewModel vModel, long id)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            IZoneTemplate obj = TemplateCache.Get<IZoneTemplate>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            obj.Name = vModel.Name;
            obj.BaseElevation = vModel.BaseElevation;
            obj.PressureCoefficient = vModel.PressureCoefficient;
            obj.TemperatureCoefficient = vModel.TemperatureCoefficient;
            obj.Hemisphere = (HemispherePlacement)vModel.Hemisphere;

            var world = TemplateCache.Get<IGaiaTemplate>(vModel.World);

            if (world == null)
            {
                message = "Error; You must choose a valid world.";
            }
            else
            {
                obj.World = world;

                if (obj.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - EditZone[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Edit Successful.";
                }
                else
                {
                    message = "Error; Edit failed.";
                }
            }
            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult AddEditLocalePath(long id, long localeId)
        {
            var locale = TemplateCache.Get<ILocaleTemplate>(localeId);

            if (locale == null)
            {
                return RedirectToAction("Edit", new { Message = "Locale has no rooms", id });
            }

            var validRooms = TemplateCache.GetAll<IRoomTemplate>().Where(rm => rm.ParentLocation.Equals(locale));

            if (validRooms.Count() == 0)
            {
                return RedirectToAction("Edit", new { Message = "Locale has no rooms", id });
            }

            var origin = TemplateCache.Get<IZoneTemplate>(id);

            var existingPathway = origin.GetLocalePathways().FirstOrDefault(path => ((IRoomTemplate)path.Destination).ParentLocation.Equals(locale));

            var vModel = new AddEditZonePathwayTemplateViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),

                ValidMaterials = TemplateCache.GetAll<IMaterial>(),
                ValidModels = TemplateCache.GetAll<IDimensionalModelData>().Where(model => model.ModelType == DimensionalModelType.Flat),
                ValidRooms = validRooms,

                Origin = origin,
                OriginID = id,

                DestinationID = -1
            };

            if (existingPathway != null)
            {
                vModel.Name = existingPathway.Name;
                vModel.Destination = (IRoomTemplate)existingPathway.Destination;
                vModel.DestinationID = existingPathway.Destination.Id;

                vModel.ModelDataObject = existingPathway.Model;

                vModel.DataObject = existingPathway;
            }

            return View("~/Views/GameAdmin/Zone/AddEditLocalePath.cshtml", vModel);
        }

        [HttpPost]
        public ActionResult AddLocalePathway(long id, AddEditZonePathwayTemplateViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var newObj = new PathwayTemplate
            {
                Name = vModel.Name,
                DegreesFromNorth = -1,
                Origin = TemplateCache.Get<IZoneTemplate>(vModel.OriginID),
                Destination = TemplateCache.Get<IRoomTemplate>(vModel.DestinationID),
            };

            if (newObj.Create(authedUser.GameAccount, authedUser.GetStaffRank(User)) == null)
            {
                message = "Error; Creation failed.";
            }
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddPathway[" + newObj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
            }

            return RedirectToAction("Edit", new { Message = message, id });
        }

        [HttpPost]
        public ActionResult EditLocalePathway(long id, AddEditZonePathwayTemplateViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());
            vModel.ValidRooms = TemplateCache.GetAll<IRoomTemplate>();

            var obj = TemplateCache.Get<IPathwayTemplate>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Edit", new { Message = message, id });
            }

            obj.Name = vModel.Name;
            obj.Destination = TemplateCache.Get<IRoomTemplate>(vModel.DestinationID);

            if (obj.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditPathwayTemplate[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
            }
            else
            {
                message = "Error; Edit failed.";
            }

            return RedirectToAction("Edit", new { Message = message, id });
        }

        [HttpGet]
        public ActionResult AddEditDescriptive(long id, short descriptiveType, string phrase)
        {
            string message = string.Empty;

            var obj = TemplateCache.Get<IZoneTemplate>(id);
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
                vModel.SensoryEventDataObject = obj.Descriptives.FirstOrDefault(occurrence => occurrence.Event.Role == grammaticalType
                                                                                        && occurrence.Event.Phrase.Equals(phrase, StringComparison.InvariantCultureIgnoreCase));
            }

            if (vModel.SensoryEventDataObject != null)
            {
                vModel.LexicaDataObject = vModel.SensoryEventDataObject.Event;
            }

            return View("~/Views/Shared/SensoryEvent.cshtml", "_chromelessLayout", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddEditDescriptive(long id, OccurrenceViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var obj = TemplateCache.Get<IZoneTemplate>(id);
            if (obj == null)
            {
                message = "That zone does not exist";
                return RedirectToRoute("ModalErrorOrClose", new { Message = message });
            }

            var grammaticalType = vModel.SensoryEventDataObject.Event.Role;
            var phraseF = vModel.SensoryEventDataObject.Event.Phrase;
            var existingOccurrence = obj.Descriptives.FirstOrDefault(occurrence => occurrence.Event.Role == grammaticalType
                                                                                && occurrence.Event.Phrase.Equals(phraseF, StringComparison.InvariantCultureIgnoreCase));

            if (existingOccurrence == null)
            {
                existingOccurrence = new SensoryEvent();
            }

            var existingEvent = existingOccurrence.Event;

            if (existingEvent == null)
            {
                existingEvent = new Lexica();
            }

            existingEvent.Role = grammaticalType;

            existingOccurrence.Event = existingEvent;

            obj.Descriptives.RemoveWhere(occ => occ.Event.Role == grammaticalType
                                                    && occ.Event.Phrase.Equals(phraseF, StringComparison.InvariantCultureIgnoreCase));
            obj.Descriptives.Add(existingOccurrence);

            if (obj.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - Zone AddEditDescriptive[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
            }
            else
            {
                message = "Error; Edit failed.";
            }

            return RedirectToRoute("ModalErrorOrClose", new { Message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveDescriptive(long id, string authorize)
        {
            string message = string.Empty;

            if (string.IsNullOrWhiteSpace(authorize))
            {
                message = "You must check the proper authorize radio button first.";
            }
            else
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());
                var values = authorize.Split(new string[] { "|||" }, StringSplitOptions.RemoveEmptyEntries);

                if (values.Count() != 2)
                {
                    message = "You must check the proper authorize radio button first.";
                }
                else
                {
                    var type = short.Parse(values[0]);
                    var phrase = values[1];

                    var obj = TemplateCache.Get<IZoneTemplate>(id);

                    if (obj == null)
                    {
                        message = "That does not exist";
                    }
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
                            {
                                message = "Error; Removal failed.";
                            }
                        }
                        else
                        {
                            message = "That does not exist";
                        }
                    }
                }
            }

            return RedirectToRoute("ModalErrorOrClose", new { Message = message });
        }

    }
}