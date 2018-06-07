using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
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
        public ActionResult Add(long id, long originRoomId, long destinationRoomId, int degreesFromNorth = 0)
        {
            //New room or existing room
            if (destinationRoomId.Equals(-1))
            {
                var vModel = new AddPathwayWithRoomDataViewModel
                {
                    authedUser = UserManager.FindById(User.Identity.GetUserId()),

                    ValidMaterials = BackingDataCache.GetAll<IMaterial>(),
                    ValidModels = BackingDataCache.GetAll<IDimensionalModelData>().Where(model => model.ModelType == DimensionalModelType.Flat),
                    ValidRooms = BackingDataCache.GetAll<IRoomData>().Where(rm => !rm.Id.Equals(originRoomId)),

                    Origin = BackingDataCache.Get<IRoomData>(originRoomId),
                    OriginID = originRoomId,

                    DegreesFromNorth = degreesFromNorth
                };

                vModel.Locale = vModel.Origin.ParentLocation;
                vModel.LocaleId = vModel.Origin.ParentLocation.Id;

                return View("~/Views/GameAdmin/Pathway/AddWithRoom.cshtml", "_chromelessLayout", vModel);
            }
            else
            {
                var vModel = new AddEditPathwayDataViewModel
                {
                    authedUser = UserManager.FindById(User.Identity.GetUserId()),

                    ValidMaterials = BackingDataCache.GetAll<IMaterial>(),
                    ValidModels = BackingDataCache.GetAll<IDimensionalModelData>().Where(model => model.ModelType == DimensionalModelType.Flat),
                    ValidRooms = BackingDataCache.GetAll<IRoomData>().Where(rm => !rm.Id.Equals(originRoomId)),

                    Origin = BackingDataCache.Get<IRoomData>(originRoomId),
                    OriginID = originRoomId,

                    DegreesFromNorth = degreesFromNorth,
                    DestinationID = destinationRoomId,
                    Destination = BackingDataCache.Get<IRoomData>(destinationRoomId)
                };

                return View("~/Views/GameAdmin/Pathway/AddEdit.cshtml", "_chromelessLayout", vModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddWithRoom(AddPathwayWithRoomDataViewModel vModel)
        {
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            string roomMessage = string.Empty;
            var newRoom = new RoomData
            {
                Name = vModel.RoomName,
                Model = new DimensionalModel(vModel.RoomDimensionalModelHeight, vModel.RoomDimensionalModelLength, vModel.RoomDimensionalModelWidth
                                , vModel.RoomDimensionalModelVacuity, vModel.RoomDimensionalModelCavitation)
            };

            var mediumId = vModel.Medium;
            var medium = BackingDataCache.Get<IMaterial>(mediumId);

            if (medium != null)
            {
                newRoom.Medium = medium;

                var locale = BackingDataCache.Get<ILocaleData>(vModel.LocaleId);

                if (locale != null)
                {
                    newRoom.ParentLocation = locale;

                    if (newRoom.Create() == null)
                        roomMessage = "Error; Creation failed.";
                    else
                    {
                        LoggingUtility.LogAdminCommandUsage("*WEB* - AddRoomDataWithPathway[" + newRoom.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    }
                }
                else
                    roomMessage = "You must include a valid Locale.";
            }
            else
                roomMessage = "You must include a valid Medium material.";

            if (!string.IsNullOrWhiteSpace(roomMessage))
                return RedirectToRoute("ModalErrorOrClose", new { Message = roomMessage });

            string message = string.Empty;
            var newObj = new PathwayData
            {
                Name = vModel.Name,
                DegreesFromNorth = vModel.DegreesFromNorth,
                Origin = BackingDataCache.Get<IRoomData>(vModel.OriginID),
                Destination = newRoom
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

            if (vModel.Strengths != null)
            {
                int lexicalIndex = 0;
                foreach (var strengthValue in vModel.Strengths)
                {
                    if (vModel.Phrases.Count() <= lexicalIndex ||
                        vModel.Roles.Count() <= lexicalIndex ||
                        vModel.Types.Count() <= lexicalIndex)
                        break;

                    var currentOccurrence = new Occurrence()
                    {
                        Strength = vModel.Strengths[lexicalIndex]
                    };

                    var lexica = new Lexica
                    {
                        Phrase = vModel.Phrases[lexicalIndex],
                        Role = (GrammaticalType)vModel.Roles[lexicalIndex],
                        Type = (LexicalType)vModel.Types[lexicalIndex]
                    };

                    int modifierIndex = -1;
                    foreach (var modifierIterator in vModel.LexicaModifierIterator)
                    {
                        modifierIndex++;

                        if (modifierIterator < lexicalIndex)
                            continue;

                        if (modifierIterator > lexicalIndex)
                            break;

                        if (!string.IsNullOrWhiteSpace(vModel.ModifierPhrases[modifierIndex]))
                        {
                            if (vModel.ModifierRoles.Count() <= modifierIndex || vModel.ModifierLexicalTypes.Count() <= modifierIndex || vModel.ModifierConjunctions.Count() <= modifierIndex)
                                break;

                            var phrase = vModel.ModifierPhrases[modifierIndex];
                            var role = (GrammaticalType)vModel.ModifierRoles[modifierIndex];
                            var type = (LexicalType)vModel.ModifierLexicalTypes[modifierIndex];
                            var conjunction = vModel.ModifierConjunctions[modifierIndex];

                            lexica.TryModify(new Lexica { Role = role, Type = type, Phrase = phrase }, conjunction);
                        }


                        modifierIndex++;
                    }

                    currentOccurrence.Event = lexica;

                    if (currentOccurrence != null || lexica != null)
                        newObj.Descriptives.Add(currentOccurrence);

                    lexicalIndex++;
                }
            }

            if (validData)
            {
                newObj.Model = new DimensionalModel(vModel.DimensionalModelHeight, vModel.DimensionalModelLength, vModel.DimensionalModelWidth,
                    vModel.DimensionalModelVacuity, vModel.DimensionalModelCavitation, new BackingDataCacheKey(dimModel), materialParts);

                if (newObj.Create() == null)
                    message = "Error; Creation failed.";
                else
                {
                    if (vModel.CreateReciprocalPath)
                    {
                        var reversePath = new PathwayData
                        {
                            Name = newObj.Name,
                            DegreesFromNorth = newObj.DegreesFromNorth,
                            Origin = newObj.Destination,
                            Destination = newObj.Origin,
                            Model = newObj.Model
                        };

                        if (reversePath.Create() == null)
                        {
                            message = "Reverse Path creation FAILED. Origin path creation SUCCESS.";
                        }
                    }

                    LoggingUtility.LogAdminCommandUsage("*WEB* - AddPathwayWithRoom[" + newObj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                }
            }

            return RedirectToRoute("ModalErrorOrClose", new { Message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(AddEditPathwayDataViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var newObj = new PathwayData
            {
                Name = vModel.Name,
                DegreesFromNorth = vModel.DegreesFromNorth,
                Origin = BackingDataCache.Get<IRoomData>(vModel.OriginID),
                Destination = BackingDataCache.Get<IRoomData>(vModel.DestinationID),
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

            if (vModel.Strengths != null)
            {
                int lexicalIndex = 0;
                foreach (var strengthValue in vModel.Strengths)
                {
                    if (vModel.Phrases.Count() <= lexicalIndex ||
                        vModel.Roles.Count() <= lexicalIndex ||
                        vModel.Types.Count() <= lexicalIndex)
                        break;

                    var currentOccurrence = new Occurrence()
                    {
                        Strength = vModel.Strengths[lexicalIndex]
                    };

                    var lexica = new Lexica
                    {
                        Phrase = vModel.Phrases[lexicalIndex],
                        Role = (GrammaticalType)vModel.Roles[lexicalIndex],
                        Type = (LexicalType)vModel.Types[lexicalIndex]
                    };

                    int modifierIndex = -1;
                    foreach (var modifierIterator in vModel.LexicaModifierIterator)
                    {
                        modifierIndex++;

                        if (modifierIterator < lexicalIndex)
                            continue;

                        if (modifierIterator > lexicalIndex)
                            break;

                        if (!string.IsNullOrWhiteSpace(vModel.ModifierPhrases[modifierIndex]))
                        {
                            if (vModel.ModifierRoles.Count() <= modifierIndex || vModel.ModifierLexicalTypes.Count() <= modifierIndex || vModel.ModifierConjunctions.Count() <= modifierIndex)
                                break;

                            var phrase = vModel.ModifierPhrases[modifierIndex];
                            var role = (GrammaticalType)vModel.ModifierRoles[modifierIndex];
                            var type = (LexicalType)vModel.ModifierLexicalTypes[modifierIndex];
                            var conjunction = vModel.ModifierConjunctions[modifierIndex];

                            lexica.TryModify(new Lexica { Role = role, Type = type, Phrase = phrase }, conjunction);
                        }


                        modifierIndex++;
                    }

                    currentOccurrence.Event = lexica;

                    if (currentOccurrence != null || lexica != null)
                        newObj.Descriptives.Add(currentOccurrence);

                    lexicalIndex++;
                }
            }

            if (validData)
            {
                newObj.Model = new DimensionalModel(vModel.DimensionalModelHeight, vModel.DimensionalModelLength, vModel.DimensionalModelWidth,
                    vModel.DimensionalModelVacuity, vModel.DimensionalModelCavitation, new BackingDataCacheKey(dimModel), materialParts);

                if (newObj.Create() == null)
                    message = "Error; Creation failed.";
                else
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - AddPathway[" + newObj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                }
            }

            return RedirectToRoute("ModalErrorOrClose", new { Message = message });
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

            vModel.OccurrenceDataObjects = obj.Descriptives.ToArray();

            vModel.ValidRooms = BackingDataCache.GetAll<IRoomData>().Where(rm => !rm.Equals(obj.Origin) && !rm.Equals(obj.Destination));

            vModel.DataObject = obj;
            vModel.Name = obj.Name;

            vModel.DegreesFromNorth = obj.DegreesFromNorth;
            vModel.Destination = (IRoomData)obj.Destination;
            vModel.Origin = (IRoomData)obj.Origin;

            vModel.DimensionalModelId = obj.Model.ModelBackingData.Id;
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

            var obj = BackingDataCache.Get<IPathwayData>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return View("~/Views/GameAdmin/Pathway/AddEdit.cshtml", vModel);
            }

            obj.Name = vModel.Name;
            obj.DegreesFromNorth = vModel.DegreesFromNorth;
            obj.Origin = BackingDataCache.Get<IRoomData>(vModel.OriginID);
            obj.Destination = BackingDataCache.Get<IRoomData>(vModel.DestinationID);

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

            var descriptives = new HashSet<IOccurrence>();
            if (vModel.Strengths != null)
            {
                int lexicalIndex = 0;
                foreach (var strengthValue in vModel.Strengths)
                {
                    if (vModel.Phrases.Count() <= lexicalIndex ||
                        vModel.Roles.Count() <= lexicalIndex ||
                        vModel.Types.Count() <= lexicalIndex)
                        break;

                    var currentOccurrence = new Occurrence()
                    {
                        Strength = vModel.Strengths[lexicalIndex]
                    };

                    var lexica = new Lexica
                    {
                        Phrase = vModel.Phrases[lexicalIndex],
                        Role = (GrammaticalType)vModel.Roles[lexicalIndex],
                        Type = (LexicalType)vModel.Types[lexicalIndex]
                    };

                    int modifierIndex = -1;
                    foreach (var modifierIterator in vModel.LexicaModifierIterator)
                    {
                        modifierIndex++;

                        if (modifierIterator < lexicalIndex)
                            continue;

                        if (modifierIterator > lexicalIndex)
                            break;

                        if (!string.IsNullOrWhiteSpace(vModel.ModifierPhrases[modifierIndex]))
                        {
                            if (vModel.ModifierRoles.Count() <= modifierIndex || vModel.ModifierLexicalTypes.Count() <= modifierIndex || vModel.ModifierConjunctions.Count() <= modifierIndex)
                                break;

                            var phrase = vModel.ModifierPhrases[modifierIndex];
                            var role = (GrammaticalType)vModel.ModifierRoles[modifierIndex];
                            var type = (LexicalType)vModel.ModifierLexicalTypes[modifierIndex];
                            var conjunction = vModel.ModifierConjunctions[modifierIndex];

                            lexica.TryModify(new Lexica { Role = role, Type = type, Phrase = phrase }, conjunction);
                        }


                        modifierIndex++;
                    }

                    currentOccurrence.Event = lexica;

                    if (currentOccurrence != null || lexica != null)
                        descriptives.Add(currentOccurrence);

                    lexicalIndex++;
                }
            }

            if (descriptives.Count == 0)
            {
                message = "At least one descriptive is required.";
                validData = false;
            }
            else
                obj.Descriptives = descriptives;

            if (validData)
            {
                obj.Model = new DimensionalModel(vModel.DimensionalModelHeight, vModel.DimensionalModelLength, vModel.DimensionalModelWidth,
                    vModel.DimensionalModelVacuity, vModel.DimensionalModelCavitation, new BackingDataCacheKey(dimModel), materialParts);

                if (obj.Save())
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - EditPathwayData[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                }
                else
                    message = "Error; Edit failed.";
            }

            return RedirectToRoute("ModalErrorOrClose", new { Message = message });
        }
    }
}