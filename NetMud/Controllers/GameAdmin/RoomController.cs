using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Communication.Lexical;
using NetMud.Data.Linguistic;
using NetMud.Data.Locale;
using NetMud.Data.Room;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
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
            ManageRoomTemplateViewModel vModel = new ManageRoomTemplateViewModel(TemplateCache.GetAll<IRoomTemplate>())
            {
                AuthedUser = UserManager.FindById(User.Identity.GetUserId()),

                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View("~/Views/GameAdmin/Room/Index.cshtml", vModel);
        }

        [HttpGet]
        public ActionResult Map(long ID)
        {
            RoomMapViewModel vModel = new RoomMapViewModel
            {
                Here = TemplateCache.Get<IRoomTemplate>(ID)
            };

            return View("~/Views/GameAdmin/Room/Map.cshtml", vModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"Room/Remove/{removeId?}/{authorizeRemove?}/{unapproveId?}/{authorizeUnapprove?}")]
        public ActionResult Remove(long removeId = -1, string authorizeRemove = "", long unapproveId = -1, string authorizeUnapprove = "")
        {
            string message = string.Empty;

            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.ToString().Equals(authorizeRemove))
            {
                ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

                IRoomTemplate obj = TemplateCache.Get<IRoomTemplate>(removeId);

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
                ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

                IRoomTemplate obj = TemplateCache.Get<IRoomTemplate>(unapproveId);

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
            ILocaleTemplate myLocale = TemplateCache.Get<ILocaleTemplate>(localeId);

            if (myLocale == null)
            {
                return RedirectToAction("Index", new { Message = "Invalid Locale" });
            }

            AddEditRoomTemplateViewModel vModel = new AddEditRoomTemplateViewModel
            {
                AuthedUser = UserManager.FindById(User.Identity.GetUserId()),
                ValidMaterials = TemplateCache.GetAll<IMaterial>(),
                ValidModels = TemplateCache.GetAll<IDimensionalModelData>(),
                ValidZones = TemplateCache.GetAll<IZoneTemplate>(),
                ValidRooms = TemplateCache.GetAll<IRoomTemplate>(),
                ValidLocales = TemplateCache.GetAll<ILocaleTemplate>().Where(locale => locale.Id != localeId),
                ZonePathway = new PathwayTemplate() { Destination = myLocale.ParentLocation },
                LocaleRoomPathway = new PathwayTemplate(),
                LocaleRoomPathwayDestinationLocale = new LocaleTemplate(),
                DataObject = new RoomTemplate() { ParentLocation = myLocale }
            };

            return View("~/Views/GameAdmin/Room/Add.cshtml", "_chromelessLayout", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(long localeId, AddEditRoomTemplateViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());
            ILocaleTemplate locale = TemplateCache.Get<ILocaleTemplate>(localeId);

            IRoomTemplate newObj = vModel.DataObject;
            newObj.ParentLocation = locale;
            newObj.Coordinates = new Coordinate(0, 0, 0); //TODO: fix this

            IPathwayTemplate zoneDestination = null;
            if (vModel.ZonePathway?.Destination != null && !string.IsNullOrWhiteSpace(vModel.ZonePathway.Name))
            {
                IZoneTemplate destination = TemplateCache.Get<IZoneTemplate>(vModel.ZonePathway.Destination.Id);
                zoneDestination = new PathwayTemplate()
                {
                    DegreesFromNorth = -1,
                    Name = vModel.ZonePathway.Name,
                    Origin = newObj,
                    Destination = destination,
                    InclineGrade = vModel.ZonePathway.InclineGrade,
                    Model = vModel.ZonePathway.Model
                };
            }

            IPathwayTemplate localeRoomPathway = null;
            if (vModel.LocaleRoomPathwayDestination != null && !string.IsNullOrWhiteSpace(vModel.LocaleRoomPathwayDestination.Name))
            {
                IRoomTemplate destination = TemplateCache.Get<IRoomTemplate>(vModel.LocaleRoomPathwayDestination.Id);
                localeRoomPathway = new PathwayTemplate()
                {
                    DegreesFromNorth = vModel.LocaleRoomPathway.DegreesFromNorth,
                    Name = vModel.LocaleRoomPathway.Name,
                    Origin = newObj,
                    Destination = destination,
                    InclineGrade = vModel.LocaleRoomPathway.InclineGrade,
                    Model = vModel.LocaleRoomPathway.Model
                };
            }


            if (newObj.Create(authedUser.GameAccount, authedUser.GetStaffRank(User)) == null)
            {
                if (zoneDestination != null)
                {
                    zoneDestination.Save(authedUser.GameAccount, authedUser.GetStaffRank(User));
                }

                if (localeRoomPathway != null)
                {
                    localeRoomPathway.Save(authedUser.GameAccount, authedUser.GetStaffRank(User));
                }

                message = "Error; Creation failed.";
            }
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddRoomTemplate[" + newObj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
            }

            return RedirectToRoute("ModalErrorOrClose", new { Message = message });
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            string message = string.Empty;
            IRoomTemplate obj = TemplateCache.Get<IRoomTemplate>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToRoute("ErrorOrClose", new { Message = message });
            }

            AddEditRoomTemplateViewModel vModel = new AddEditRoomTemplateViewModel
            {
                AuthedUser = UserManager.FindById(User.Identity.GetUserId()),
                ValidMaterials = TemplateCache.GetAll<IMaterial>(),
                ValidZones = TemplateCache.GetAll<IZoneTemplate>(),
                ValidLocales = TemplateCache.GetAll<ILocaleTemplate>().Where(locale => locale.Id != obj.ParentLocation.Id),
                ValidLocaleRooms = TemplateCache.GetAll<IRoomTemplate>().Where(room => room.Id != obj.Id && room.ParentLocation.Id == obj.ParentLocation.Id),
                ValidRooms = TemplateCache.GetAll<IRoomTemplate>().Where(room => room.Id != obj.Id),
                ValidModels = TemplateCache.GetAll<IDimensionalModelData>(),
                DataObject = obj,
            };

            IPathwayTemplate zoneDestination = obj.GetZonePathways().FirstOrDefault();
            if (zoneDestination != null)
            {
                vModel.ZonePathway = zoneDestination;
            }
            else
            {
                vModel.ZonePathway = new PathwayTemplate() { Destination = obj.ParentLocation.ParentLocation, Origin = obj };
            }

            IPathwayTemplate localeRoomPathway = obj.GetLocalePathways().FirstOrDefault();
            if (localeRoomPathway != null)
            {
                vModel.LocaleRoomPathway = localeRoomPathway;
                vModel.LocaleRoomPathwayDestinationLocale = ((IRoomTemplate)localeRoomPathway.Destination).ParentLocation;
                vModel.ValidLocaleRooms = TemplateCache.GetAll<IRoomTemplate>().Where(room => localeRoomPathway.Id == room.ParentLocation.Id);
            }
            else
            {
                vModel.LocaleRoomPathway = new PathwayTemplate() { Origin = obj };
                vModel.LocaleRoomPathwayDestinationLocale = new LocaleTemplate();
            }


            return View("~/Views/GameAdmin/Room/Edit.cshtml", "_chromelessLayout", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, AddEditRoomTemplateViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());
            IPathwayTemplate zoneDestination = null;
            IPathwayTemplate localeRoomPathway = null;

            IRoomTemplate obj = TemplateCache.Get<IRoomTemplate>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToRoute("ModalErrorOrClose", new { Message = message });
            }

            obj.Name = vModel.DataObject.Name;
            obj.Medium = vModel.DataObject.Medium;
            obj.Qualities = vModel.DataObject.Qualities;

            if (vModel.ZonePathway?.Destination != null && !string.IsNullOrWhiteSpace(vModel.ZonePathway.Name))
            {
                IZoneTemplate destination = TemplateCache.Get<IZoneTemplate>(vModel.ZonePathway.Destination.Id);
                zoneDestination = obj.GetZonePathways().FirstOrDefault();

                if (zoneDestination == null)
                {
                    zoneDestination = new PathwayTemplate()
                    {
                        DegreesFromNorth = vModel.ZonePathway.DegreesFromNorth,
                        Name = vModel.ZonePathway.Name,
                        Origin = obj,
                        Destination = destination,
                        InclineGrade = vModel.ZonePathway.InclineGrade,
                        Model = vModel.ZonePathway.Model
                    };
                }
                else
                {
                    zoneDestination.Model = vModel.ZonePathway.Model;
                    zoneDestination.Name = vModel.ZonePathway.Name;
                    zoneDestination.InclineGrade = vModel.ZonePathway.InclineGrade;

                    //We switched zones, this makes things more complicated
                    if (zoneDestination.Id != vModel.ZonePathway.Destination.Id)
                    {
                        zoneDestination.Destination = destination;
                    }
                }
            }

            if (vModel.LocaleRoomPathwayDestination != null && !string.IsNullOrWhiteSpace(vModel.LocaleRoomPathwayDestination.Name))
            {
                IRoomTemplate destination = TemplateCache.Get<IRoomTemplate>(vModel.LocaleRoomPathwayDestination.Id);
                localeRoomPathway = obj.GetLocalePathways().FirstOrDefault();

                if (localeRoomPathway == null)
                {
                    localeRoomPathway = new PathwayTemplate()
                    {
                        DegreesFromNorth = vModel.LocaleRoomPathway.DegreesFromNorth,
                        Name = vModel.LocaleRoomPathway.Name,
                        Origin = obj,
                        Destination = destination,
                        InclineGrade = vModel.LocaleRoomPathway.InclineGrade,
                        Model = vModel.LocaleRoomPathway.Model
                    };
                }
                else
                {
                    localeRoomPathway.Model = vModel.LocaleRoomPathway.Model;
                    localeRoomPathway.Name = vModel.LocaleRoomPathway.Name;
                    localeRoomPathway.InclineGrade = vModel.LocaleRoomPathway.InclineGrade;
                    localeRoomPathway.Destination = destination;
                }
            }

            if (obj.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
            {
                if (zoneDestination != null)
                {
                    zoneDestination.Save(authedUser.GameAccount, authedUser.GetStaffRank(User));
                }

                if (localeRoomPathway != null)
                {
                    localeRoomPathway.Save(authedUser.GameAccount, authedUser.GetStaffRank(User));
                }

                LoggingUtility.LogAdminCommandUsage("*WEB* - EditRoomTemplate[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
            }
            else
            {
                message = "Error; Edit failed.";
            }

            return RedirectToRoute("ModalErrorOrClose");
        }

        #region Descriptives
        [HttpGet]
        public ActionResult AddEditDescriptive(long id, short descriptiveType, string phrase)
        {
            string message = string.Empty;

            IRoomTemplate obj = TemplateCache.Get<IRoomTemplate>(id);
            if (obj == null)
            {
                message = "That room does not exist";
                return RedirectToRoute("ModalErrorOrClose", new { Message = message });
            }

            OccurrenceViewModel vModel = new OccurrenceViewModel
            {
                AuthedUser = UserManager.FindById(User.Identity.GetUserId()),
                DataObject = obj,
                AdminTypeName = "Room"
            };

            if (descriptiveType > -1)
            {
                GrammaticalType grammaticalType = (GrammaticalType)descriptiveType;
                vModel.SensoryEventDataObject = obj.Descriptives.FirstOrDefault(occurrence => occurrence.Event.Role == grammaticalType
                                                                                        && occurrence.Event.Phrase.Equals(phrase, StringComparison.InvariantCultureIgnoreCase));
            }

            if (vModel.SensoryEventDataObject != null)
            {
                vModel.LexicaDataObject = vModel.SensoryEventDataObject.Event;
            }
            else
            {
                vModel.SensoryEventDataObject = new SensoryEvent
                {
                    Event = new Lexica()
                };
            }

            return View("~/Views/GameAdmin/Room/SensoryEvent.cshtml", "_chromelessLayout", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddEditDescriptive(long id, OccurrenceViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            IRoomTemplate obj = TemplateCache.Get<IRoomTemplate>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToRoute("ModalErrorOrClose", new { Message = message });
            }

            ISensoryEvent existingOccurrence = obj.Descriptives.FirstOrDefault(occurrence => occurrence.Event.Role == vModel.SensoryEventDataObject.Event.Role
                                                                                && occurrence.Event.Phrase.Equals(vModel.SensoryEventDataObject.Event.Phrase, StringComparison.InvariantCultureIgnoreCase));

            if (existingOccurrence == null)
            {
                existingOccurrence = new SensoryEvent(vModel.SensoryEventDataObject.SensoryType)
                {
                    Strength = vModel.SensoryEventDataObject.Strength,
                    Event = new Lexica(vModel.SensoryEventDataObject.Event.Type,
                                        vModel.SensoryEventDataObject.Event.Role,
                                        vModel.SensoryEventDataObject.Event.Phrase, new LexicalContext())
                    {
                        Modifiers = vModel.SensoryEventDataObject.Event.Modifiers
                    }
                };
            }
            else
            {
                existingOccurrence.Strength = vModel.SensoryEventDataObject.Strength;
                existingOccurrence.SensoryType = vModel.SensoryEventDataObject.SensoryType;
                existingOccurrence.Event = new Lexica(vModel.SensoryEventDataObject.Event.Type,
                                                        vModel.SensoryEventDataObject.Event.Role,
                                                        vModel.SensoryEventDataObject.Event.Phrase, new LexicalContext())
                {
                    Modifiers = vModel.SensoryEventDataObject.Event.Modifiers
                };
            }

            obj.Descriptives.RemoveWhere(occurrence => occurrence.Event.Role == vModel.SensoryEventDataObject.Event.Role
                                                && occurrence.Event.Phrase.Equals(vModel.SensoryEventDataObject.Event.Phrase, StringComparison.InvariantCultureIgnoreCase));

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
                ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());
                string[] values = authorize.Split(new string[] { "|||" }, StringSplitOptions.RemoveEmptyEntries);

                if (values.Count() != 2)
                {
                    message = "You must check the proper authorize radio button first.";
                }
                else
                {
                    short type = short.Parse(values[0]);
                    string phrase = values[1];

                    IRoomTemplate obj = TemplateCache.Get<IRoomTemplate>(id);

                    if (obj == null)
                    {
                        message = "That does not exist";
                    }
                    else
                    {
                        GrammaticalType grammaticalType = (GrammaticalType)type;
                        ISensoryEvent existingOccurrence = obj.Descriptives.FirstOrDefault(occurrence => occurrence.Event.Role == grammaticalType
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
        #endregion
    }
}