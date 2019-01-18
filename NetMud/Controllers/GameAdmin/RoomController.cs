using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Communication.Lexical;
using NetMud.Data.Architectural.EntityBase;
using NetMud.Data.Linguistic;
using NetMud.Data.Room;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.Locale;
using NetMud.DataStructure.Room;
using NetMud.DataStructure.System;
using NetMud.DataStructure.Zone;
using NetMud.Models.Admin;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers.GameAdmin
{
    [Authorize(Roles = "Admin,Builder")]
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
            var vModel = new ManageRoomTemplateViewModel(TemplateCache.GetAll<IRoomTemplate>())
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),

                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View("~/Views/GameAdmin/Room/Index.cshtml", vModel);
        }

        [HttpGet]
        public ActionResult Map(long ID)
        {
            var vModel = new RoomMapViewModel
            {
                Here = TemplateCache.Get<IRoomTemplate>(ID)
            };

            return View("~/Views/GameAdmin/Room/Map.cshtml", vModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"GameAdmin/Room/Remove/{removeId?}/{authorizeRemove?}/{unapproveId?}/{authorizeUnapprove?}")]
        public ActionResult Remove(long removeId = -1, string authorizeRemove = "", long unapproveId = -1, string authorizeUnapprove = "")
        {
            string message = string.Empty;

            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.ToString().Equals(authorizeRemove))
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = TemplateCache.Get<IRoomTemplate>(removeId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.Remove(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveRoom[" + removeId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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

                var obj = TemplateCache.Get<IRoomTemplate>(unapproveId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.ChangeApprovalStatus(authedUser.GameAccount, authedUser.GetStaffRank(User), ApprovalState.Returned))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - UnapproveRoom[" + unapproveId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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
        public ActionResult Add(long localeId)
        {
            var vModel = new AddEditRoomTemplateViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                ValidMaterials = TemplateCache.GetAll<IMaterial>(),
                Locale = TemplateCache.Get<ILocaleTemplate>(localeId)
            };

            return View("~/Views/GameAdmin/Room/Add.cshtml", "_chromelessLayout", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(long localeId, AddEditRoomTemplateViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());
            var locale = TemplateCache.Get<ILocaleTemplate>(localeId);

            var newObj = new RoomTemplate
            {
                Name = vModel.Name
            };

            var mediumId = vModel.Medium;
            var medium = TemplateCache.Get<IMaterial>(mediumId);

            if (medium != null)
            {
                newObj.Medium = medium;
                newObj.ParentLocation = locale;
                newObj.Coordinates = new Coordinate(0, 0, 0); //TODO: fix this

                if (newObj.Create(authedUser.GameAccount, authedUser.GetStaffRank(User)) == null)
                {
                    message = "Error; Creation failed.";
                }
                else
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - AddRoomTemplate[" + newObj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                }
            }
            else
            {
                message = "You must include a valid Medium material.";
            }

            return RedirectToRoute("ModalErrorOrClose", new { Message = message });
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            string message = string.Empty;
            var vModel = new AddEditRoomTemplateViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                ValidMaterials = TemplateCache.GetAll<IMaterial>(),
                ValidZones = TemplateCache.GetAll<IZoneTemplate>(),
            };

            var obj = TemplateCache.Get<IRoomTemplate>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToRoute("ErrorOrClose", new { Message = message });
            }

            vModel.Locale = obj.ParentLocation;
            vModel.DataObject = obj;
            vModel.Name = obj.Name;

            var zoneDestination = obj.GetZonePathways().FirstOrDefault();

            if (zoneDestination != null)
            {
                vModel.ZonePathway = zoneDestination;
                vModel.ZoneDestinationId = zoneDestination.Destination.Id;
                vModel.ZonePathwayName = zoneDestination.Name;
                vModel.ZoneDimensionalModelHeight = zoneDestination.Model.Height;
                vModel.ZoneDimensionalModelLength = zoneDestination.Model.Length;
                vModel.ZoneDimensionalModelWidth = zoneDestination.Model.Width;
                vModel.ZoneDimensionalModelCavitation = zoneDestination.Model.SurfaceCavitation;
                vModel.ZoneDimensionalModelVacuity = zoneDestination.Model.Vacuity;
            }
            else
            {
                vModel.ZoneDestinationId = -1;
            }

            return View("~/Views/GameAdmin/Room/Edit.cshtml", "_chromelessLayout", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, AddEditRoomTemplateViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());
            IPathwayTemplate zoneDestination = null;

            var obj = TemplateCache.Get<IRoomTemplate>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToRoute("ModalErrorOrClose", new { Message = message });
            }

            var mediumId = vModel.Medium;
            var medium = TemplateCache.Get<IMaterial>(mediumId);

            if (medium != null)
            {
                obj.Name = vModel.Name;
                obj.Medium = medium;

                var destination = TemplateCache.Get<IZoneTemplate>(vModel.ZoneDestinationId);
                if (destination != null)
                {
                    zoneDestination = obj.GetZonePathways().FirstOrDefault();

                    if (zoneDestination == null)
                    {
                        zoneDestination = new PathwayTemplate(new DimensionalModel(vModel.ZoneDimensionalModelLength, vModel.ZoneDimensionalModelHeight, vModel.ZoneDimensionalModelWidth,
                                                                                vModel.ZoneDimensionalModelVacuity, vModel.ZoneDimensionalModelCavitation))
                        {
                            DegreesFromNorth = -1,
                            Name = vModel.ZonePathwayName,
                            Origin = obj,
                            Destination = destination
                        };
                    }
                    else
                    {
                        zoneDestination.Model = new DimensionalModel(vModel.ZoneDimensionalModelLength, vModel.ZoneDimensionalModelHeight, vModel.ZoneDimensionalModelWidth,
                                                    vModel.ZoneDimensionalModelVacuity, vModel.ZoneDimensionalModelCavitation);
                        zoneDestination.Name = vModel.ZonePathwayName;

                        //We switched zones, this makes things more complicated
                        if (zoneDestination.Id != vModel.ZoneDestinationId)
                        {
                            zoneDestination.Destination = destination;
                        }
                    }

                }

                if (obj.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    if (zoneDestination != null)
                    {
                        zoneDestination.Save(authedUser.GameAccount, authedUser.GetStaffRank(User));
                    }

                    LoggingUtility.LogAdminCommandUsage("*WEB* - EditRoomTemplate[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                }
                else
                {
                    message = "Error; Edit failed.";
                }
            }
            else
            {
                message = "You must include a valid Medium material.";
            }

            return RedirectToRoute("ModalErrorOrClose");
        }

        [HttpGet]
        public ActionResult AddEditDescriptive(long id, short descriptiveType, string phrase)
        {
            string message = string.Empty;

            var obj = TemplateCache.Get<IRoomTemplate>(id);
            if (obj == null)
            {
                message = "That room does not exist";
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
                                                                                        && occurrence.Event.Phrase.Equals(phrase, System.StringComparison.InvariantCultureIgnoreCase));
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
                message = "That room does not exist";
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
                LoggingUtility.LogAdminCommandUsage("*WEB* - Room AddEditDescriptive[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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

                    var obj = TemplateCache.Get<IRoomTemplate>(id);

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