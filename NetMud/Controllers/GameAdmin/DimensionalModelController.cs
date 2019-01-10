using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Data.Architectural.EntityBase;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.Models.Admin;
using System;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers.GameAdmin
{
    [Authorize(Roles = "Admin,Builder")]
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
            var vModel = new ManageDimensionalModelDataViewModel(TemplateCache.GetAll<DimensionalModelData>())
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
        [Route(@"GameAdmin/DimensionalModel/Remove/{removeId?}/{authorizeRemove?}/{unapproveId?}/{authorizeUnapprove?}")]
        public ActionResult Remove(long removeId = -1, string authorizeRemove = "", long unapproveId = -1, string authorizeUnapprove = "")
        {
            string message = string.Empty;

            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.ToString().Equals(authorizeRemove))
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = TemplateCache.Get<IDimensionalModelData>(removeId);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.Remove(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveDimensionalModelData[" + removeId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                    message = "Error; Removal failed.";
            }
            else if (!string.IsNullOrWhiteSpace(authorizeUnapprove) && unapproveId.ToString().Equals(authorizeUnapprove))
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = TemplateCache.Get<IDimensionalModelData>(unapproveId);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.ChangeApprovalStatus(authedUser.GameAccount, authedUser.GetStaffRank(User), ApprovalState.Returned))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - UnapproveDimensionalModelData[" + unapproveId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Unapproval Successful.";
                }
                else
                    message = "Error; Unapproval failed.";
            }
            else
                message = "You must check the proper remove or unapprove authorization radio button first.";

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
                else if(vModel.ModelPlaneNames.Count(m => !string.IsNullOrEmpty(m)) == 21
                    && vModel.CoordinateDamageTypes.Any(m => !m.Equals(0))) //can't have an entirely null typed model
                {
                    //We're going to be cheaty and build a cDel string based on the arrays
                    var arrayString = new StringBuilder();

                    var i = 21;
                    foreach(var name in vModel.ModelPlaneNames.Reverse())
                    {
                        arrayString.AppendLine(
                            string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21}"
                                , name
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 1]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 2]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 3]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 4]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 5]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 6]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 7]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 8]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 9]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 10]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 11]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 12]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 13]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 14]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 15]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 16]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 17]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 18]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 19]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 20]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 21]))
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
                        message = "Invalid model file; Model files must contain 21 planes of a tag name followed by 21 rows of 21 nodes.";
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

            var obj = TemplateCache.Get<IDimensionalModelData>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            vModel.DataObject = obj;
            vModel.Name = obj.Name;
            vModel.ModelType = obj.ModelType;

            return View("~/Views/GameAdmin/DimensionalModel/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(long id, AddEditDimensionalModelDataViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var obj = TemplateCache.Get<IDimensionalModelData>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            try
            {
                DimensionalModelData newModel = null;
                
                if (vModel.ModelPlaneNames.Count(m => !string.IsNullOrEmpty(m)) == 21
                    && vModel.CoordinateDamageTypes.Any(m => !m.Equals(0))) //can't have an entirely null typed model
                {
                    //We're going to be cheaty and build a cDel string based on the arrays
                    var arrayString = new StringBuilder();

                    var i = 21;
                    foreach (var name in vModel.ModelPlaneNames.Reverse())
                    {
                        arrayString.AppendLine(
                            string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21}"
                                , name
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 1]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 2]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 3]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 4]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 5]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 6]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 7]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 8]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 9]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 10]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 11]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 12]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 13]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 14]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 15]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 16]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 17]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 18]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 19]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 20]))
                                , Physics.Render.DamageTypeToCharacter(((DamageType)vModel.CoordinateDamageTypes[i * 21 - 21]))
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
                        message = "Invalid model; Models must contain 21 planes of a tag name followed by 21 rows of 21 nodes.";
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