using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Communication.Messaging;
using NetMud.Data.EntityBackingData;
using NetMud.Data.LookupData;
using NetMud.Data.System;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Models.Admin;
using System;
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
            var vModel = new ManageRoomDataViewModel(BackingDataCache.GetAll<IRoomData>())
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
                Here = BackingDataCache.Get<IRoomData>(ID)
            };

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
        public ActionResult Add(long localeId)
        {
            var vModel = new AddEditRoomDataViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                ValidMaterials = BackingDataCache.GetAll<IMaterial>(),
                Locale = BackingDataCache.Get<ILocaleData>(localeId)
            };

            return View("~/Views/GameAdmin/Room/Add.cshtml", "_chromelessLayout", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(long localeId, AddEditRoomDataViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());
            var locale = BackingDataCache.Get<ILocaleData>(localeId);

            var newObj = new RoomData
            {
                Name = vModel.Name,
                Model = new DimensionalModel(vModel.DimensionalModelHeight, vModel.DimensionalModelLength, vModel.DimensionalModelWidth
                                , vModel.DimensionalModelVacuity, vModel.DimensionalModelCavitation),
            };

            var mediumId = vModel.Medium;
            var medium = BackingDataCache.Get<IMaterial>(mediumId);

            if (medium != null)
            {
                newObj.Medium = medium;
                newObj.ParentLocation = locale;
                newObj.Coordinates = new Tuple<int, int, int>(0, 0, 0); //TODO: fix this

                if (newObj.Create() == null)
                    message = "Error; Creation failed.";
                else
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - AddRoomData[" + newObj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                }
            }
            else
                message = "You must include a valid Medium material.";

            return RedirectToRoute("ModalErrorOrClose", new { Message = message });
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            string message = string.Empty;
            var vModel = new AddEditRoomDataViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                ValidMaterials = BackingDataCache.GetAll<IMaterial>(),
                ValidZones = BackingDataCache.GetAll<IZoneData>(),
            };

            var obj = BackingDataCache.Get<RoomData>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToRoute("ErrorOrClose", new { Message = message });
            }

            vModel.Locale = obj.ParentLocation;
            vModel.DataObject = obj;
            vModel.Name = obj.Name;
            vModel.DimensionalModelHeight = obj.Model.Height;
            vModel.DimensionalModelLength = obj.Model.Length;
            vModel.DimensionalModelWidth = obj.Model.Width;
            vModel.DimensionalModelCavitation = obj.Model.SurfaceCavitation;
            vModel.DimensionalModelVacuity = obj.Model.Vacuity;

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
                vModel.ZoneDestinationId = -1;

            return View("~/Views/GameAdmin/Room/Edit.cshtml", "_chromelessLayout", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, AddEditRoomDataViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());
            IPathwayData zoneDestination = null;

            var obj = BackingDataCache.Get<RoomData>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToRoute("ModalErrorOrClose", new { Message = message });
            }

            var mediumId = vModel.Medium;
            var medium = BackingDataCache.Get<IMaterial>(mediumId);

            if (medium != null)
            {
                obj.Name = vModel.Name;
                obj.Model.Height = vModel.DimensionalModelHeight;
                obj.Model.Length = vModel.DimensionalModelLength;
                obj.Model.Width = vModel.DimensionalModelWidth;
                obj.Model.SurfaceCavitation = vModel.DimensionalModelCavitation;
                obj.Model.Vacuity = vModel.DimensionalModelVacuity;
                obj.Medium = medium;

                var destination = BackingDataCache.Get<IZoneData>(vModel.ZoneDestinationId);
                if (destination != null)
                {
                    zoneDestination = obj.GetZonePathways().FirstOrDefault();
                    
                    if(zoneDestination == null)
                    {
                        zoneDestination = new PathwayData(new DimensionalModel(vModel.ZoneDimensionalModelLength, vModel.ZoneDimensionalModelHeight, vModel.ZoneDimensionalModelWidth,
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

                if (obj.Save())
                {
                    if (zoneDestination != null)
                        zoneDestination.Save();

                    LoggingUtility.LogAdminCommandUsage("*WEB* - EditRoomData[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                }
                else
                    message = "Error; Edit failed.";
            }
            else
                message = "You must include a valid Medium material.";

            return RedirectToRoute("ModalErrorOrClose");
        }

        [HttpGet]
        public ActionResult AddEditDescriptive(long id, short descriptiveType, string phrase)
        {
            string message = string.Empty;

            var obj = BackingDataCache.Get<IRoomData>(id);
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
                vModel.OccurrenceDataObject = obj.Descriptives.FirstOrDefault(occurrence => occurrence.Event.Role == grammaticalType
                                                                                        && occurrence.Event.Phrase.Equals(phrase, System.StringComparison.InvariantCultureIgnoreCase));
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

            return View("~/Views/Shared/Occurrence.cshtml", "_chromelessLayout", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddEditDescriptive(long id, OccurrenceViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var obj = BackingDataCache.Get<IZoneData>(id);
            if (obj == null)
            {
                message = "That room does not exist";
                return RedirectToRoute("ModalErrorOrClose", new { Message = message });
            }

            var grammaticalType = (GrammaticalType)vModel.Role;
            var phraseF = vModel.Phrase;
            var existingOccurrence = obj.Descriptives.FirstOrDefault(occurrence => occurrence.Event.Role == grammaticalType
                                                                                && occurrence.Event.Phrase.Equals(phraseF, System.StringComparison.InvariantCultureIgnoreCase));

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

            if (obj.Save())
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - Room AddEditDescriptive[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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

                    var obj = BackingDataCache.Get<IRoomData>(id);

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

                            if (obj.Save())
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