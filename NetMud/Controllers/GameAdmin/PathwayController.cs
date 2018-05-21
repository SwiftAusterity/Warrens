using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Data.EntityBackingData;
using NetMud.Data.LookupData;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.Supporting;
using NetMud.Models.Admin;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers.GameAdmin
{
    public class PathwayController : Controller
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

        public PathwayController()
        {
        }

        public PathwayController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Remove(long ID, string authorize)
        {
            var vModel = new AddEditPathwayDataViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId())
            };

            string message = string.Empty;

            if (string.IsNullOrWhiteSpace(authorize) || !ID.ToString().Equals(authorize))
                message = "You must check the proper authorize radio button first.";
            else
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = BackingDataCache.Get<PathwayData>(ID);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.Remove())
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemovePathway[" + ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                    message = "Error; Removal failed.";
            }

            return View("~/Views/GameAdmin/Pathway/AddEdit.cshtml", vModel);
        }

        [HttpGet]
        public ActionResult Add(long id, long originRoomId, long destinationRoomId)
        {
            var vModel = new AddEditPathwayDataViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),

                ValidMaterials = BackingDataCache.GetAll<IMaterial>(),
                ValidModels = BackingDataCache.GetAll<IDimensionalModelData>().Where(model => model.ModelType == DimensionalModelType.Flat),
                ValidRooms = BackingDataCache.GetAll<IRoomData>().Where(rm => !rm.ID.Equals(originRoomId)),

                Origin = BackingDataCache.Get<IRoomData>(originRoomId),
                OriginID = originRoomId
            };

            //New room or existing room
            if (destinationRoomId.Equals(-1))
            {
                vModel.RoomModel.authedUser = vModel.authedUser;
                vModel.RoomModel.ValidMaterials = vModel.ValidMaterials;

                return View("~/Views/GameAdmin/Pathway/AddWithRoom.cshtml", "_chromelessLayout", vModel);
            }
            else
            {
                vModel.DestinationID = destinationRoomId;
                vModel.Destination = BackingDataCache.Get<IRoomData>(destinationRoomId);

                return View("~/Views/GameAdmin/Pathway/AddEdit.cshtml", "_chromelessLayout", vModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddWithRoom(AddEditPathwayDataViewModel vModel, long id)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var newObj = new PathwayData
            {
                Name = vModel.Name,
                DegreesFromNorth = vModel.DegreesFromNorth,
                Origin = vModel.ValidRooms.FirstOrDefault(room => room.ID.Equals(vModel.OriginID)),
                Destination = vModel.ValidRooms.FirstOrDefault(room => room.ID.Equals(vModel.DestinationID)),
            };

            var materialParts = new Dictionary<string, IMaterial>();
            if (vModel.ModelPartNames != null)
            {
                int nameIndex = 0;
                foreach (var partName in vModel.ModelPartNames)
                {
                    if (!string.IsNullOrWhiteSpace(partName))
                    {
                        if (vModel.ModelPartMaterials.Count() <= nameIndex)
                            break;

                        var material = BackingDataCache.Get<Material>(vModel.ModelPartMaterials[nameIndex]);

                        if (material != null && !string.IsNullOrWhiteSpace(partName))
                            materialParts.Add(partName, material);
                    }

                    nameIndex++;
                }
            }

            var dimModel = BackingDataCache.Get<DimensionalModelData>(vModel.DimensionalModelId);
            bool validData = true;

            if (dimModel == null)
            {
                message = "Choose a valid dimensional model.";
                validData = false;
            }

            if (dimModel.ModelPlanes.Any(plane => !materialParts.ContainsKey(plane.TagName)))
            {
                message = "You need to choose a material for each Dimensional Model planar section. (" + string.Join(",", dimModel.ModelPlanes.Select(plane => plane.TagName)) + ")";
                validData = false;
            }

            if (validData)
            {
                newObj.Model = new DimensionalModel(vModel.DimensionalModelHeight, vModel.DimensionalModelLength, vModel.DimensionalModelWidth,
                    vModel.DimensionalModelVacuity, vModel.DimensionalModelCavitation, vModel.DimensionalModelId, materialParts);

                if (newObj.Create() == null)
                    message = "Error; Creation failed.";
                else
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - AddPathwayWithRoom[" + newObj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Creation Successful.";
                }
            }

            string roomMessage = string.Empty;
            var newRoom = new RoomData
            {
                Name = vModel.RoomModel.Name,
                Model = new DimensionalModel(vModel.RoomModel.DimensionalModelHeight, vModel.RoomModel.DimensionalModelLength, vModel.RoomModel.DimensionalModelWidth
                                , vModel.RoomModel.DimensionalModelVacuity, vModel.RoomModel.DimensionalModelCavitation)
            };

            var mediumId = vModel.RoomModel.Medium;
            var medium = BackingDataCache.Get<IMaterial>(mediumId);

            if (medium != null)
            {
                newRoom.Medium = medium;

                var locale = vModel.RoomModel.Locale;

                if (locale != null)
                {
                    newRoom.ParentLocation = locale;

                    if (newRoom.Create() == null)
                        roomMessage = "Error; Creation failed.";
                    else
                    {
                        LoggingUtility.LogAdminCommandUsage("*WEB* - AddRoomDataWithPathway[" + newRoom.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                        roomMessage = "Creation Successful.";
                    }
                }
                else
                    roomMessage = "You must include a valid Zone.";
            }
            else
                roomMessage = "You must include a valid Medium material.";

            var result = new ContentResult
            {
                Content = message + "|" + roomMessage
            };

            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(AddEditPathwayDataViewModel vModel, long id)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var newObj = new PathwayData
            {
                Name = vModel.Name,
                DegreesFromNorth = vModel.DegreesFromNorth,
                Origin = vModel.ValidRooms.FirstOrDefault(room => room.ID.Equals(vModel.OriginID)),
                Destination = vModel.ValidRooms.FirstOrDefault(room => room.ID.Equals(vModel.OriginID)),
            };

            var materialParts = new Dictionary<string, IMaterial>();
            if (vModel.ModelPartNames != null)
            {
                int nameIndex = 0;
                foreach (var partName in vModel.ModelPartNames)
                {
                    if (!string.IsNullOrWhiteSpace(partName))
                    {
                        if (vModel.ModelPartMaterials.Count() <= nameIndex)
                            break;

                        var material = BackingDataCache.Get<IMaterial>(vModel.ModelPartMaterials[nameIndex]);

                        if (material != null && !string.IsNullOrWhiteSpace(partName))
                            materialParts.Add(partName, material);
                    }

                    nameIndex++;
                }
            }

            var dimModel = BackingDataCache.Get<IDimensionalModelData>(vModel.DimensionalModelId);
            bool validData = true;

            if (dimModel == null)
            {
                message = "Choose a valid dimensional model.";
                validData = false;
            }

            if (dimModel.ModelPlanes.Any(plane => !materialParts.ContainsKey(plane.TagName)))
            {
                message = "You need to choose a material for each Dimensional Model planar section. (" + string.Join(",", dimModel.ModelPlanes.Select(plane => plane.TagName)) + ")";
                validData = false;
            }

            if (validData)
            {
                newObj.Model = new DimensionalModel(vModel.DimensionalModelHeight, vModel.DimensionalModelLength, vModel.DimensionalModelWidth, 
                    vModel.DimensionalModelVacuity, vModel.DimensionalModelCavitation, vModel.DimensionalModelId, materialParts);

                if (newObj.Create() == null)
                    message = "Error; Creation failed.";
                else
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - AddPathway[" + newObj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Creation Successful.";
                }
            }

            var result = new ContentResult
            {
                Content = message
            };

            return result;
        }

        [HttpGet]
        public ActionResult Edit(long id, long originRoomId, long destinationRoomId)
        {
            string message = string.Empty;
            var vModel = new AddEditPathwayDataViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),

                ValidMaterials = BackingDataCache.GetAll<IMaterial>(),
                ValidModels = BackingDataCache.GetAll<IDimensionalModelData>().Where(model => model.ModelType == DimensionalModelType.Flat)
            };

            var obj = BackingDataCache.Get<PathwayData>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", "Room", new { Message = message });
            }

            vModel.ValidRooms = BackingDataCache.GetAll<IRoomData>().Where(rm => !rm.Equals(obj.Origin) && !rm.Equals(obj.Destination));

            vModel.DataObject = obj;
            vModel.Name = obj.Name;

            vModel.DegreesFromNorth = obj.DegreesFromNorth;
            vModel.Destination = (IRoomData)obj.Destination;
            vModel.Origin = (IRoomData)obj.Origin;

            vModel.DimensionalModelId = obj.Model.ModelBackingData.ID;
            vModel.DimensionalModelHeight = obj.Model.Height;
            vModel.DimensionalModelLength = obj.Model.Length;
            vModel.DimensionalModelWidth = obj.Model.Width;
            vModel.DimensionalModelVacuity = obj.Model.Vacuity;
            vModel.DimensionalModelCavitation = obj.Model.SurfaceCavitation;
            vModel.ModelDataObject = obj.Model;

            return View("~/Views/GameAdmin/Pathway/AddEdit.cshtml", "_chromelessLayout", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(long id, AddEditPathwayDataViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());
            vModel.ValidRooms = BackingDataCache.GetAll<IRoomData>();

            var obj = BackingDataCache.Get<IPathwayData>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return View("~/Views/GameAdmin/Pathway/AddEdit.cshtml", vModel);
            }

            obj.Name = vModel.Name;
            obj.DegreesFromNorth = vModel.DegreesFromNorth;
            obj.Origin = vModel.ValidRooms.FirstOrDefault(room => room.ID.Equals(vModel.OriginID));
            obj.Destination = vModel.ValidRooms.FirstOrDefault(room => room.ID.Equals(vModel.DestinationID));

            var materialParts = new Dictionary<string, IMaterial>();
            if (vModel.ModelPartNames != null)
            {
                int nameIndex = 0;
                foreach (var partName in vModel.ModelPartNames)
                {
                    if (!string.IsNullOrWhiteSpace(partName))
                    {
                        if (vModel.ModelPartMaterials.Count() <= nameIndex)
                            break;

                        var material = BackingDataCache.Get<Material>(vModel.ModelPartMaterials[nameIndex]);

                        if (material != null)
                            materialParts.Add(partName, material);
                    }

                    nameIndex++;
                }
            }

            var dimModel = BackingDataCache.Get<DimensionalModelData>(vModel.DimensionalModelId);
            bool validData = true;

            if (dimModel == null)
            {
                message = "Choose a valid dimensional model.";
                validData = false;
            }

            if (dimModel.ModelPlanes.Any(plane => !materialParts.ContainsKey(plane.TagName)))
            {
                message = "You need to choose a material for each Dimensional Model planar section. (" + string.Join(",", dimModel.ModelPlanes.Select(plane => plane.TagName)) + ")";
                validData = false;
            }

            if (validData)
            {
                obj.Model = new DimensionalModel(vModel.DimensionalModelHeight, vModel.DimensionalModelLength, vModel.DimensionalModelWidth, 
                    vModel.DimensionalModelVacuity, vModel.DimensionalModelCavitation, vModel.DimensionalModelId, materialParts);

                if (obj.Save())
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - EditPathwayData[" + obj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Edit Successful.";
                }
                else
                    message = "Error; Edit failed.";
            }

            var result = new ContentResult
            {
                Content = message
            };

            //Don't return to the room editor, this is in a window, it just needs to close
            return result;
        }
    }
}