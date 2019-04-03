using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Communication.Lexical;
using NetMud.Data.Gaia;
using NetMud.Data.Linguistic;
using NetMud.Data.Zone;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Gaia;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.Locale;
using NetMud.DataStructure.NPC;
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
    public class LiveAdminController : Controller
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

        public LiveAdminController()
        {
        }

        public LiveAdminController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        #region Zone
        [HttpGet]
        public ActionResult Zones(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            LiveZonesViewModel vModel = new LiveZonesViewModel(LiveCache.GetAll<IZone>())
            {
                AuthedUser = UserManager.FindById(User.Identity.GetUserId()),
                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View(vModel);
        }

        [HttpGet]
        [Route(@"LiveAdmin/Zone")]
        public ActionResult Zone(string birthMark, ViewZoneViewModel viewModel)
        {
            var authedUser = UserManager.FindById(User.Identity.GetUserId());
            ViewZoneViewModel vModel = new ViewZoneViewModel(birthMark)
            {
                AuthedUser = authedUser,
                Elegance = viewModel.Elegance,
                Severity = viewModel.Severity,
                Quality = viewModel.Quality,
                Language = viewModel.Language ?? authedUser.GameAccount.Config.UILanguage
            };

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"LiveAdmin/EditZone")]
        public ActionResult EditZone(string birthMark, ViewZoneViewModel vModel)
        {
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            IZone obj = LiveCache.Get<IZone>(new LiveCacheKey(typeof(Zone), birthMark));
            string message;
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            obj.BaseElevation = vModel.DataObject.BaseElevation;
            obj.Hemisphere = vModel.DataObject.Hemisphere;
            obj.Humidity = vModel.DataObject.Humidity;
            obj.Temperature = vModel.DataObject.Temperature;

            //obj.NaturalResources = vModel.DataObject.NaturalResources;
            obj.Qualities = vModel.DataObject.Qualities;

            if (obj.Save())
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - LIVE DATA - EditZone[" + obj.BirthMark + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
            {
                message = "Error; Edit failed.";
            }

            return RedirectToAction("Zone", new { Message = message, birthMark });
        }

        [HttpGet]
        [Route(@"LiveAdmin/Zone/AddEditDescriptive")]
        public ActionResult AddEditZoneDescriptive(string birthMark, short descriptiveType, string phrase)
        {
            string message = string.Empty;

            IZone obj = LiveCache.Get<IZone>(new LiveCacheKey(typeof(Zone), birthMark));
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToRoute("ModalErrorOrClose", new { Message = message });
            }

            LiveOccurrenceViewModel vModel = new LiveOccurrenceViewModel
            {
                AuthedUser = UserManager.FindById(User.Identity.GetUserId()),
                DataObject = obj,
                AdminTypeName = "LiveAdmin/Zone"
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

            return View("~/Views/LiveAdmin/Zone/SensoryEvent.cshtml", "_chromelessLayout", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"LiveAdmin/Zone/AddEditDescriptive")]
        public ActionResult AddEditZoneDescriptive(string birthMark, LiveOccurrenceViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            IZone obj = LiveCache.Get<IZone>(new LiveCacheKey(typeof(Zone), birthMark));
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
                                        vModel.SensoryEventDataObject.Event.Phrase, new LexicalContext(null))
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
                                                        vModel.SensoryEventDataObject.Event.Phrase, new LexicalContext(null))
                {
                    Modifiers = vModel.SensoryEventDataObject.Event.Modifiers
                };
            }

            obj.Descriptives.RemoveWhere(occurrence => occurrence.Event.Role == vModel.SensoryEventDataObject.Event.Role
                                                && occurrence.Event.Phrase.Equals(vModel.SensoryEventDataObject.Event.Phrase, StringComparison.InvariantCultureIgnoreCase));

            obj.Descriptives.Add(existingOccurrence);

            if (obj.Save())
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - LIVE DATA - Zone AddEditDescriptive[" + obj.BirthMark + "]", authedUser.GameAccount.GlobalIdentityHandle);
            }
            else
            {
                message = "Error; Edit failed.";
            }

            return RedirectToRoute("ModalErrorOrClose", new { Message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"LiveAdmin/Zone/SensoryEvent/Remove/{id?}/{authorize?}")]
        public ActionResult RemoveZoneDescriptive(string id = "", string authorize = "")
        {
            string message = string.Empty;
            string zoneId = "";

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
                    string type = values[0];
                    string phrase = values[1];

                    var obj = LiveCache.Get<IZone>(new LiveCacheKey(typeof(IZone), id));

                    if (obj == null)
                    {
                        message = "That does not exist";
                    }
                    else
                    {
                        GrammaticalType grammaticalType = (GrammaticalType)Enum.Parse(typeof(GrammaticalType), type);
                        ISensoryEvent existingOccurrence = obj.Descriptives.FirstOrDefault(occurrence => occurrence.Event.Role == grammaticalType
                                                                                            && occurrence.Event.Phrase.Equals(phrase, StringComparison.InvariantCultureIgnoreCase));
                        zoneId = obj.BirthMark;

                        if (existingOccurrence != null)
                        {
                            obj.Descriptives.Remove(existingOccurrence);

                            if (obj.Save())
                            {
                                LoggingUtility.LogAdminCommandUsage("*WEB* - LIVE DATA - RemoveDescriptive[" + id.ToString() + "|" + type.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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

            return RedirectToAction("Zone", new { Message = message, birthMark = id });
        }
        #endregion

        #region World
        [HttpGet]
        public ActionResult Worlds(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            LiveWorldsViewModel vModel = new LiveWorldsViewModel(LiveCache.GetAll<IGaia>())
            {
                AuthedUser = UserManager.FindById(User.Identity.GetUserId()),
                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View(vModel);
        }

        [HttpGet]
        [Route(@"LiveAdmin/World")]
        public ActionResult World(string birthMark, ViewZoneViewModel viewModel)
        {
            var authedUser = UserManager.FindById(User.Identity.GetUserId());
            ViewGaiaViewModel vModel = new ViewGaiaViewModel(birthMark)
            {
                AuthedUser = authedUser,
                Elegance = viewModel.Elegance,
                Severity = viewModel.Severity,
                Quality = viewModel.Quality,
                Language = viewModel.Language ?? authedUser.GameAccount.Config.UILanguage
            };

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"LiveAdmin/EditWorld")]
        public ActionResult EditWorld(string birthMark, ViewGaiaViewModel vModel)
        {
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            IGaia obj = LiveCache.Get<IGaia>(new LiveCacheKey(typeof(Gaia), birthMark));
            string message;
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            obj.RotationalAngle = vModel.DataObject.RotationalAngle;
            obj.OrbitalPosition = vModel.DataObject.OrbitalPosition;
            obj.Macroeconomy = vModel.DataObject.Macroeconomy;
            obj.CelestialPositions = vModel.DataObject.CelestialPositions;
            obj.MeterologicalFronts = vModel.DataObject.MeterologicalFronts;
            obj.CurrentTimeOfDay = vModel.DataObject.CurrentTimeOfDay;

            obj.Qualities = vModel.DataObject.Qualities;

            if (obj.Save())
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - LIVE DATA - EditGaia[" + obj.BirthMark + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
            {
                message = "Error; Edit failed.";
            }

            return RedirectToAction("World", new { Message = message, birthMark });
        }
        #endregion

        #region items
        [HttpGet]
        public ActionResult Inanimates(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            LiveInanimatesViewModel vModel = new LiveInanimatesViewModel(LiveCache.GetAll<IInanimate>())
            {
                AuthedUser = UserManager.FindById(User.Identity.GetUserId()),
                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View(vModel);
        }

        [HttpGet]
        [Route(@"LiveAdmin/Inanimate")]
        public ActionResult Inanimate(string birthMark, ViewZoneViewModel viewModel)
        {
            var authedUser = UserManager.FindById(User.Identity.GetUserId());
            ViewInanimateViewModel vModel = new ViewInanimateViewModel(birthMark)
            {
                AuthedUser = authedUser,
                Elegance = viewModel.Elegance,
                Severity = viewModel.Severity,
                Quality = viewModel.Quality,
                Language = viewModel.Language ?? authedUser.GameAccount.Config.UILanguage
            };

            return View(vModel);
        }
        #endregion

        #region NPC
        [HttpGet]
        public ActionResult NPCs(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            LiveNPCsViewModel vModel = new LiveNPCsViewModel(LiveCache.GetAll<INonPlayerCharacter>())
            {
                AuthedUser = UserManager.FindById(User.Identity.GetUserId()),
                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View(vModel);
        }

        [HttpGet]
        [Route(@"LiveAdmin/NPC")]
        public ActionResult NPC(string birthMark, ViewZoneViewModel viewModel)
        {
            var authedUser = UserManager.FindById(User.Identity.GetUserId());
            ViewIntelligenceViewModel vModel = new ViewIntelligenceViewModel(birthMark)
            {
                AuthedUser = authedUser,
                Elegance = viewModel.Elegance,
                Severity = viewModel.Severity,
                Quality = viewModel.Quality,
                Language = viewModel.Language ?? authedUser.GameAccount.Config.UILanguage
            };

            return View(vModel);
        }
        #endregion

        #region Room
        [HttpGet]
        public ActionResult Rooms(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            LiveRoomsViewModel vModel = new LiveRoomsViewModel(LiveCache.GetAll<IRoom>())
            {
                AuthedUser = UserManager.FindById(User.Identity.GetUserId()),
                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View(vModel);
        }

        [HttpGet]
        [Route(@"LiveAdmin/Room")]
        public ActionResult Room(string birthMark, ViewZoneViewModel viewModel)
        {
            var authedUser = UserManager.FindById(User.Identity.GetUserId());
            ViewRoomViewModel vModel = new ViewRoomViewModel(birthMark)
            {
                AuthedUser = authedUser,
                Elegance = viewModel.Elegance,
                Severity = viewModel.Severity,
                Quality = viewModel.Quality,
                Language = viewModel.Language ?? authedUser.GameAccount.Config.UILanguage
            };

            return View(vModel);
        }
        #endregion

        #region Locale
        [HttpGet]
        public ActionResult Locales(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            LiveLocalesViewModel vModel = new LiveLocalesViewModel(LiveCache.GetAll<ILocale>())
            {
                AuthedUser = UserManager.FindById(User.Identity.GetUserId()),
                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View(vModel);
        }

        [HttpGet]
        [Route(@"LiveAdmin/Locale")]
        public ActionResult Locale(string birthMark, ViewZoneViewModel viewModel)
        {
            var authedUser = UserManager.FindById(User.Identity.GetUserId());
            ViewLocaleViewModel vModel = new ViewLocaleViewModel(birthMark)
            {
                AuthedUser = authedUser,
                Elegance = viewModel.Elegance,
                Severity = viewModel.Severity,
                Quality = viewModel.Quality,
                Language = viewModel.Language ?? authedUser.GameAccount.Config.UILanguage
            };

            return View(vModel);
        }
        #endregion
    }
}