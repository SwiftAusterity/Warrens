using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Data.Zones;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Gaia;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.NPC;
using NetMud.DataStructure.Tile;
using NetMud.DataStructure.Zone;
using NetMud.Models.Admin;
using System;
using System.Collections.Generic;
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
            ManageZoneDataViewModel vModel = new ManageZoneDataViewModel(TemplateCache.GetAll<IZoneTemplate>())
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
                ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = TemplateCache.Get<IZoneTemplate>(removeId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.Remove(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    var liveObj = LiveCache.Get<IZone>(removeId);

                    if (liveObj != null)
                    {
                        liveObj.Remove();
                    }

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
                ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = TemplateCache.Get<IZoneTemplate>(unapproveId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.ChangeApprovalStatus(authedUser.GameAccount, authedUser.GetStaffRank(User), ApprovalState.Unapproved))
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
        public ActionResult Add(long Template = -1)
        {
            AddEditZoneDataViewModel vModel = new AddEditZoneDataViewModel(Template)
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                ValidTileTypes = TemplateCache.GetAll<ITileTemplate>(true),
                ValidWorlds = TemplateCache.GetAll<IGaiaTemplate>(true),
                ValidItems = TemplateCache.GetAll<IInanimateTemplate>(true),
                ValidNPCs = TemplateCache.GetAll<INonPlayerCharacterTemplate>(true),
                DataObject = new ZoneTemplate()
            };

            return View("~/Views/GameAdmin/Zone/Add.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(AddEditZoneDataViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            ZoneTemplate newObj = new ZoneTemplate
            {
                Name = vModel.DataObject.Name,
                Hemisphere = vModel.DataObject.Hemisphere,
                AsciiCharacter = "0",
                BaseTileType = vModel.DataObject.BaseTileType,
                BackgroundHexColor = vModel.DataObject.BackgroundHexColor,
                Font = vModel.DataObject.Font,
                Description = vModel.DataObject.Description,
                BaseBiome = vModel.DataObject.BaseBiome,
                PressureCoefficient = vModel.DataObject.PressureCoefficient,
                TemperatureCoefficient = vModel.DataObject.TemperatureCoefficient,
                BaseCoordinates = vModel.DataObject.BaseCoordinates,
                World = vModel.DataObject.World,
                Map = vModel.DataObject.Map
            };

            if (newObj.World == null)
            {
                message = "Error; You must choose a valid world.";
            }
            else
            {
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

            if (obj.Map == null)
                obj.Map = new ZoneTemplateMap();

            AddEditZoneDataViewModel vModel = new AddEditZoneDataViewModel(-1)
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                DataObject = obj,
                ValidWorlds = TemplateCache.GetAll<IGaiaTemplate>(true),
                ValidTileTypes = TemplateCache.GetAll<ITileTemplate>(true),
                ValidItems = TemplateCache.GetAll<IInanimateTemplate>(true),
                ValidNPCs = TemplateCache.GetAll<INonPlayerCharacterTemplate>(true),
            };

            return View("~/Views/GameAdmin/Zone/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AddEditZoneDataViewModel vModel, long id)
        {
            string message = string.Empty;
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            IZoneTemplate obj = TemplateCache.Get<IZoneTemplate>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            obj.Name = vModel.DataObject.Name;
            obj.Hemisphere = vModel.DataObject.Hemisphere;
            obj.BaseTileType = vModel.DataObject.BaseTileType;
            obj.BackgroundHexColor = vModel.DataObject.BackgroundHexColor;
            obj.Font = vModel.DataObject.Font;
            obj.Description = vModel.DataObject.Description;
            obj.BaseBiome = vModel.DataObject.BaseBiome;
            obj.PressureCoefficient = vModel.DataObject.PressureCoefficient;
            obj.TemperatureCoefficient = vModel.DataObject.TemperatureCoefficient;
            obj.BaseCoordinates = vModel.DataObject.BaseCoordinates;
            obj.World = vModel.DataObject.World;
            obj.Map = vModel.DataObject.Map;

            if (obj.World == null)
            {
                message = "Error; You must choose a valid world.";
            }
            else
            {
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
        public ActionResult AddZonePathway(long id)
        {
            var origin = TemplateCache.Get<IZoneTemplate>(id);

            if (origin == null)
            {
                return RedirectToAction("Index", new { Message = "Invalid Zone" });
            }

            AddZonePathwayDataViewModel vModel = new AddZonePathwayDataViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                ValidZones = TemplateCache.GetAll<IZoneTemplate>(),
                Origin = origin
            };

            return View("~/Views/GameAdmin/Zone/AddZonePathway.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddZonePathway(long id, AddZonePathwayDataViewModel vModel)
        {
            var origin = TemplateCache.Get<IZoneTemplate>(id);

            if (origin == null)
            {
                return RedirectToAction("Index", new { Message = "Invalid Zone" });
            }

            string message = string.Empty;
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            if (origin.Pathways.Any(path => path.OriginCoordinates.X == vModel.OriginCoordinateX && path.OriginCoordinates.Y == vModel.OriginCoordinateY))
            {
                message = "Pathway already exists at origin coordinates. Choose new coordinates or remove the existing pathway first.";
            }
            else
            {
                PathwayData newObj = new PathwayData
                {
                    OriginCoordinates = new Coordinate(vModel.OriginCoordinateX, vModel.OriginCoordinateY),
                    BorderHexColor = vModel.BorderHexColor
                };

                HashSet<IPathwayDestination> destinations = new HashSet<IPathwayDestination>();
                if (vModel.DestinationId != null)
                {
                    int icIndex = 0;
                    foreach (long dId in vModel.DestinationId)
                    {
                        if (dId >= 0)
                        {
                            if (vModel.DestinationCoordinateX.Count() <= icIndex || vModel.DestinationCoordinateY.Count() <= icIndex || vModel.DestinationName.Count() <= icIndex)
                            {
                                break;
                            }

                            var destinationZone = TemplateCache.Get<IZoneTemplate>(dId);

                            if (destinationZone != null
                                && vModel.DestinationCoordinateX[icIndex] >= 0 && vModel.DestinationCoordinateX[icIndex] <= 100
                                && vModel.DestinationCoordinateY[icIndex] >= 0 && vModel.DestinationCoordinateY[icIndex] <= 100
                                && !string.IsNullOrWhiteSpace(vModel.DestinationName[icIndex]))
                            {
                                destinations.Add(new PathwayDestination()
                                {
                                    Destination = destinationZone,
                                    Coordinates = new Coordinate(vModel.DestinationCoordinateX[icIndex], vModel.DestinationCoordinateY[icIndex]),
                                    Name = vModel.DestinationName[icIndex]
                                });
                            }
                        }

                        icIndex++;
                    }
                }

                newObj.Destinations = destinations;

                origin.Pathways.Add(newObj);

                if (!origin.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    message = "Error; Creation failed.";
                }
                else
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - AddZonePathway[" + id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                }
            }

            return RedirectToAction("Edit", new { Message = message, id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"GameAdmin/Zone/RemoveZonePathway/{removePathwayId?}/{authorizeRemovePathway?}")]
        public ActionResult RemoveZonePathway(long removePathwayId = -1, string authorizeRemovePathway = "")
        {
            string message = string.Empty;

            if (string.IsNullOrWhiteSpace(authorizeRemovePathway))
            {
                message = "You must check the proper authorize radio button first.";
            }
            else
            {
                ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());
                string[] values = authorizeRemovePathway.Split(new string[] { "|||" }, StringSplitOptions.RemoveEmptyEntries);

                if (values.Count() != 2)
                {
                    message = "You must check the proper authorize radio button first.";
                }
                else
                {
                    long originX = long.Parse(values[0]);
                    long originY = long.Parse(values[1]);

                    var origin = TemplateCache.Get<IZoneTemplate>(removePathwayId);

                    if (origin == null)
                    {
                        message = "That zone does not exist";
                    }
                    else
                    {
                        var existingPath = origin.Pathways.FirstOrDefault(path => path.OriginCoordinates.X == originX && path.OriginCoordinates.Y == originY);

                        if (existingPath != null)
                        {
                            origin.Pathways.Remove(existingPath);

                            if (origin.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                            {
                                LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveZonePath[" + removePathwayId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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

            return RedirectToAction("Edit", new { Message = message, id = removePathwayId });
        }
    }
}