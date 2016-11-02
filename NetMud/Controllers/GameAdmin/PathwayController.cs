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
using NetMud.Utility;
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
            var vModel = new AddEditPathwayDataViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());

            string message = string.Empty;
            long roomId = -1;

            if (string.IsNullOrWhiteSpace(authorize) || !ID.ToString().Equals(authorize))
                message = "You must check the proper authorize radio button first.";
            else
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = BackingDataCache.Get<PathwayData>(ID);
                roomId = DataUtility.TryConvert<long>(obj.FromLocationID);

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
            var vModel = new AddEditPathwayDataViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());

            vModel.ValidMaterials = BackingDataCache.GetAll<Material>();
            vModel.ValidModels = BackingDataCache.GetAll<DimensionalModelData>().Where(model => model.ModelType == DimensionalModelType.Flat);
            vModel.ValidRooms = BackingDataCache.GetAll<RoomData>().Where(rm => !rm.ID.Equals(originRoomId));

            vModel.FromLocation = BackingDataCache.Get<IRoomData>(originRoomId);
            vModel.FromLocationID = originRoomId;

            //New room or existing room
            if(destinationRoomId.Equals(-1))
            {
                vModel.NewRoomModel.authedUser = vModel.authedUser;
                vModel.NewRoomModel.ValidMaterials = vModel.ValidMaterials;
                vModel.NewRoomModel.ValidZones = BackingDataCache.GetAll<IZone>();

                return PartialView("~/Views/GameAdmin/Pathway/AddWithRoom.cshtml", vModel);
            }
            else
            {
                vModel.ToLocationID = destinationRoomId;
                vModel.ToLocation = BackingDataCache.Get<IRoomData>(destinationRoomId);

                return PartialView("~/Views/GameAdmin/Pathway/AddEdit.cshtml", vModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddWithRoom(AddEditPathwayDataViewModel vModel, long id)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var newObj = new PathwayData();
            newObj.Name = vModel.NewName;
            newObj.AudibleStrength = vModel.AudibleStrength;
            newObj.AudibleToSurroundings = vModel.AudibleToSurroundings;
            newObj.DegreesFromNorth = vModel.DegreesFromNorth;
            newObj.FromLocationID = vModel.ToLocationID.ToString();
            newObj.FromLocationType = "RoomData";
            newObj.MessageToActor = vModel.MessageToActor;
            newObj.MessageToDestination = vModel.MessageToDestination;
            newObj.MessageToOrigin = vModel.MessageToOrigin;
            newObj.ToLocationID = vModel.ToLocationID.ToString();
            newObj.ToLocationType = "RoomData";
            newObj.VisibleStrength = vModel.VisibleStrength;
            newObj.VisibleToSurroundings = vModel.VisibleToSurroundings;

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
            var newRoom = new RoomData();
            newRoom.Name = vModel.NewRoomModel.NewName;
            newRoom.Model = new DimensionalModel(vModel.NewRoomModel.DimensionalModelHeight, vModel.NewRoomModel.DimensionalModelLength, vModel.NewRoomModel.DimensionalModelWidth
                                , vModel.NewRoomModel.DimensionalModelVacuity, vModel.NewRoomModel.DimensionalModelCavitation);

            var mediumId = vModel.NewRoomModel.Medium;
            var medium = BackingDataCache.Get<IMaterial>(mediumId);

            if (medium != null)
            {
                newRoom.Medium = medium;

                var zoneId = vModel.NewRoomModel.Zone;
                var zone = BackingDataCache.Get<IZone>(zoneId);

                if (zone != null)
                {
                    newRoom.ZoneAffiliation = zone;

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

            var result = new ContentResult();
            result.Content = message + "|" + roomMessage;

            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(AddEditPathwayDataViewModel vModel, long id)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var newObj = new PathwayData();
            newObj.Name = vModel.NewName;
            newObj.AudibleStrength = vModel.AudibleStrength;
            newObj.AudibleToSurroundings = vModel.AudibleToSurroundings;
            newObj.DegreesFromNorth = vModel.DegreesFromNorth;
            newObj.FromLocationID = vModel.ToLocationID.ToString();
            newObj.FromLocationType = "RoomData";
            newObj.MessageToActor = vModel.MessageToActor;
            newObj.MessageToDestination = vModel.MessageToDestination;
            newObj.MessageToOrigin = vModel.MessageToOrigin;
            newObj.ToLocationID = vModel.ToLocationID.ToString();
            newObj.ToLocationType = "RoomData";
            newObj.VisibleStrength = vModel.VisibleStrength;
            newObj.VisibleToSurroundings = vModel.VisibleToSurroundings;

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
                    LoggingUtility.LogAdminCommandUsage("*WEB* - AddPathway[" + newObj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Creation Successful.";
                }
            }

            var result = new ContentResult();
            result.Content = message;

            return result;
        }

        [HttpGet]
        public ActionResult Edit(long id, long originRoomId, long destinationRoomId)
        {
            string message = string.Empty;
            var vModel = new AddEditPathwayDataViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());

            vModel.ValidMaterials = BackingDataCache.GetAll<Material>();
            vModel.ValidModels = BackingDataCache.GetAll<DimensionalModelData>().Where(model => model.ModelType == DimensionalModelType.Flat);

            var obj = BackingDataCache.Get<PathwayData>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", "Room", new { Message = message });
            }

            vModel.ValidRooms = BackingDataCache.GetAll<RoomData>().Where(rm => !rm.ID.Equals(obj.FromLocationID) && !rm.ID.Equals(obj.ToLocationID));

            vModel.DataObject = obj;
            vModel.NewName = obj.Name;

            vModel.AudibleStrength = obj.AudibleStrength;
            vModel.AudibleToSurroundings = obj.AudibleToSurroundings;
            vModel.DegreesFromNorth = obj.DegreesFromNorth;
            vModel.MessageToActor = obj.MessageToActor;
            vModel.MessageToDestination = obj.MessageToDestination;
            vModel.MessageToOrigin = obj.MessageToOrigin;
            vModel.ToLocationID = DataUtility.TryConvert<long>(obj.ToLocationID);
            vModel.ToLocation = BackingDataCache.Get<IRoomData>(DataUtility.TryConvert<long>(obj.ToLocationID));
            vModel.FromLocationID = DataUtility.TryConvert<long>(obj.FromLocationID);
            vModel.FromLocation = BackingDataCache.Get<IRoomData>(DataUtility.TryConvert<long>(obj.FromLocationID));
            vModel.VisibleStrength = obj.VisibleStrength;
            vModel.VisibleToSurroundings = obj.VisibleToSurroundings;

            vModel.DimensionalModelId = obj.Model.ModelBackingData.ID;
            vModel.DimensionalModelHeight = obj.Model.Height;
            vModel.DimensionalModelLength = obj.Model.Length;
            vModel.DimensionalModelWidth = obj.Model.Width;
            vModel.DimensionalModelVacuity = obj.Model.Vacuity;
            vModel.DimensionalModelCavitation = obj.Model.SurfaceCavitation;
            vModel.ModelDataObject = obj.Model;

            return PartialView("~/Views/GameAdmin/Pathway/AddEdit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(long id, AddEditPathwayDataViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var obj = BackingDataCache.Get<IPathwayData>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return View("~/Views/GameAdmin/Pathway/AddEdit.cshtml", vModel);
            }

            obj.Name = vModel.NewName;
            obj.AudibleStrength = vModel.AudibleStrength;
            obj.AudibleToSurroundings = vModel.AudibleToSurroundings;
            obj.DegreesFromNorth = vModel.DegreesFromNorth;
            obj.FromLocationID = vModel.FromLocationID.ToString();
            obj.FromLocationType = "RoomData";
            obj.MessageToActor = vModel.MessageToActor;
            obj.MessageToDestination = vModel.MessageToDestination;
            obj.MessageToOrigin = vModel.MessageToOrigin;
            obj.ToLocationID = vModel.ToLocationID.ToString();
            obj.ToLocationType = "RoomData";
            obj.VisibleStrength = vModel.VisibleStrength;
            obj.VisibleToSurroundings = vModel.VisibleToSurroundings;

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
            
            var result = new ContentResult();
            result.Content = message;

            //Don't return to the room editor, this is in a window, it just needs to close
            return result;
        }
    }
}