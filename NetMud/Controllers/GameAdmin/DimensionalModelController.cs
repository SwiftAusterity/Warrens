using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Data.LookupData;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Supporting;
using NetMud.Models.Admin;
using System;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers.GameAdmin
{
    public class DimensionalModelController : Controller
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

        public DimensionalModelController()
        {
        }

        public DimensionalModelController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ActionResult Index(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            var vModel = new ManageDimensionalModelDataViewModel(BackingDataCache.GetAll<DimensionalModelData>())
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),

                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View("~/Views/GameAdmin/DimensionalModel/Index.cshtml", vModel);
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

                var obj = BackingDataCache.Get<DimensionalModelData>(ID);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.Remove(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveDimensionalModelData[" + ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                    message = "Error; Removal failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult Add()
        {
            var vModel = new AddEditDimensionalModelDataViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId())
            };

            return View("~/Views/GameAdmin/DimensionalModel/Add.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(AddEditDimensionalModelDataViewModel vModel, HttpPostedFileBase modelFile)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            try
            {
                DimensionalModelData newModel = null;

                //So we have file OR manual now so file trumps manual
                if (modelFile != null && modelFile.ContentLength > 0)
                {
                    byte[] bytes = new byte[modelFile.InputStream.Length];
                    modelFile.InputStream.Read(bytes, 0, (int)modelFile.InputStream.Length);
                    var fileContents = Encoding.UTF8.GetString(bytes);

                    newModel = new DimensionalModelData(fileContents, vModel.ModelType);
                }
                else if(vModel.ModelPlaneNames.Count(m => !string.IsNullOrEmpty(m)) == 11
                    && vModel.CoordinateDamageTypes.Any(m => !m.Equals(0))) //can't have an entirely null typed model
                {
                    //We're going to be cheaty and build a cDel string based on the arrays
                    var arrayString = new StringBuilder();

                    var i = 11;
                    foreach(var name in vModel.ModelPlaneNames.Reverse())
                    {
                        arrayString.AppendLine(
                            string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}"
                                , name
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 11 - 1]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 11 - 2]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 11 - 3]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 11 - 4]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 11 - 5]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 11 - 6]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 11 - 7]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 11 - 8]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 11 - 9]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 11 - 10]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 11 - 11]))
                            )
                        );

                        i--;
                    }

                    newModel = new DimensionalModelData(arrayString.ToString(), vModel.ModelType);
                }
                else
                    message = "You must post a comma delimited file with the model in it or use the manual form.";

                if (newModel != null)
                {
                    newModel.Name = vModel.Name;
                    newModel.HelpText = vModel.HelpText;

                    if (newModel.IsModelValid())
                    {
                        if (newModel.Create(authedUser.GameAccount, authedUser.GetStaffRank(User)) == null)
                            message = "Error; Creation failed.";
                        else
                        {
                            LoggingUtility.LogAdminCommandUsage("*WEB* - AddDimensionalModelData[" + newModel.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                            message = "Creation Successful.";
                        }
                    }
                    else
                        message = "Invalid model file; Model files must contain 11 planes of a tag name followed by 11 rows of 11 nodes.";
                }
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex, false);
                message = "Error; Creation failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }


        [HttpGet]
        public ActionResult Edit(long id)
        {
            string message = string.Empty;
            var vModel = new AddEditDimensionalModelDataViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId())
            };

            var obj = BackingDataCache.Get<IDimensionalModelData>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            vModel.DataObject = obj;
            vModel.Name = obj.Name;
            vModel.ModelType = obj.ModelType;
            vModel.HelpText = obj.HelpText;

            return View("~/Views/GameAdmin/DimensionalModel/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(long id, AddEditDimensionalModelDataViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var obj = BackingDataCache.Get<IDimensionalModelData>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            try
            {
                DimensionalModelData newModel = null;
                
                if (vModel.ModelPlaneNames.Count(m => !string.IsNullOrEmpty(m)) == 11
                    && vModel.CoordinateDamageTypes.Any(m => !m.Equals(0))) //can't have an entirely null typed model
                {
                    //We're going to be cheaty and build a cDel string based on the arrays
                    var arrayString = new StringBuilder();

                    var i = 11;
                    foreach (var name in vModel.ModelPlaneNames.Reverse())
                    {
                        arrayString.AppendLine(
                            string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}"
                                , name
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 11 - 1]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 11 - 2]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 11 - 3]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 11 - 4]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 11 - 5]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 11 - 6]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 11 - 7]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 11 - 8]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 11 - 9]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 11 - 10]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 11 - 11]))
                            )
                        );

                        i--;
                    }

                    newModel = new DimensionalModelData(arrayString.ToString(), vModel.ModelType);
                }
                else
                    message = "You must post a comma delimited file with the model in it or use the manual form.";

                if (newModel != null)
                {
                    if (newModel.IsModelValid())
                    {
                        obj.Name = vModel.Name;
                        obj.HelpText = vModel.HelpText;
                        obj.ModelType = newModel.ModelType;
                        obj.ModelPlanes = newModel.ModelPlanes;

                        if (obj.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                        {
                            LoggingUtility.LogAdminCommandUsage("*WEB* - EditDimensionalModelData[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                            message = "Edit Successful.";
                        }
                        else
                            message = "Error; Edit failed.";
                    }
                    else
                        message = "Invalid model; Models must contain 11 planes of a tag name followed by 11 rows of 11 nodes.";
                }
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex, false);
                message = "Error; Creation failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }
    }
}