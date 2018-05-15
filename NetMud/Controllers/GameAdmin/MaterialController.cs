using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Data.LookupData;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Supporting;
using NetMud.Models.Admin;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers.GameAdmin
{
    public class MaterialController : Controller
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

        public MaterialController()
        {
        }

        public MaterialController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ActionResult Index(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            var vModel = new ManageMaterialDataViewModel(BackingDataCache.GetAll<Material>())
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),

                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View("~/Views/GameAdmin/Material/Index.cshtml", vModel);
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

                var obj = BackingDataCache.Get<Material>(ID);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.Remove())
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveMaterial[" + ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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
            var vModel = new AddEditMaterialViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                ValidMaterials = BackingDataCache.GetAll<IMaterial>()
            };

            return View("~/Views/GameAdmin/Material/Add.cshtml", vModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(AddEditMaterialViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var newObj = new Material
            {
                Name = vModel.Name,
                Conductive = vModel.Conductive,
                Density = vModel.Density,
                Ductility = vModel.Ductility,
                Flammable = vModel.Flammable,
                GasPoint = vModel.GasPoint,
                Magnetic = vModel.Magnetic,
                Mallebility = vModel.Mallebility,
                Porosity = vModel.Porosity,
                SolidPoint = vModel.SolidPoint,
                TemperatureRetention = vModel.TemperatureRetention,
                Viscosity = vModel.Viscosity,
                HelpText = vModel.HelpBody
            };

            if (vModel.Resistances != null)
            {
                int resistancesIndex = 0;
                foreach (var type in vModel.Resistances)
                {
                    if (type > 0)
                    {
                        if (vModel.ResistanceValues.Count() <= resistancesIndex)
                            break;

                        var currentValue = vModel.ResistanceValues[resistancesIndex];

                        if (currentValue > 0)
                            newObj.Resistance.Add((DamageType)type, currentValue);
                    }

                    resistancesIndex++;
                }
            }

            if (vModel.Compositions != null)
            {
                int compositionsIndex = 0;
                foreach (var materialId in vModel.Compositions)
                {
                    if (materialId > 0)
                    {
                        if (vModel.CompositionPercentages.Count() <= compositionsIndex)
                            break;

                        var currentValue = vModel.CompositionPercentages[compositionsIndex];
                        var material = BackingDataCache.Get<Material>(materialId);

                        if (material != null && currentValue > 0)
                            newObj.Composition.Add(material, currentValue);
                    }

                    compositionsIndex++;
                }
            }

            if (newObj.Create() == null)
                message = "Error; Creation failed.";
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddMaterialData[" + newObj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Creation Successful.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            string message = string.Empty;
            var vModel = new AddEditMaterialViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                ValidMaterials = BackingDataCache.GetAll<Material>()
            };

            var obj = BackingDataCache.Get<Material>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            vModel.DataObject = obj;
            vModel.Name = obj.Name;
            vModel.Conductive = obj.Conductive;
            vModel.Density = obj.Density;
            vModel.Ductility = obj.Ductility;
            vModel.Flammable = obj.Flammable;
            vModel.GasPoint = obj.GasPoint;
            vModel.Magnetic = obj.Magnetic;
            vModel.Mallebility = obj.Mallebility;
            vModel.Porosity = obj.Porosity;
            vModel.SolidPoint = obj.SolidPoint;
            vModel.TemperatureRetention = obj.TemperatureRetention;
            vModel.Viscosity = obj.Viscosity;
            vModel.HelpBody = obj.HelpText;

            return View("~/Views/GameAdmin/Material/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, AddEditMaterialViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var obj = BackingDataCache.Get<Material>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            obj.Name = vModel.Name;
            obj.Conductive = vModel.Conductive;
            obj.Density = vModel.Density;
            obj.Ductility = vModel.Ductility;
            obj.Flammable = vModel.Flammable;
            obj.GasPoint = vModel.GasPoint;
            obj.Magnetic = vModel.Magnetic;
            obj.Mallebility = vModel.Mallebility;
            obj.Porosity = vModel.Porosity;
            obj.SolidPoint = vModel.SolidPoint;
            obj.TemperatureRetention = vModel.TemperatureRetention;
            obj.Viscosity = vModel.Viscosity;
            obj.HelpText = vModel.HelpBody;

            if (vModel.Resistances != null)
            {
                int resistancesIndex = 0;
                foreach (var type in vModel.Resistances)
                {
                    if (type > 0)
                    {
                        if (vModel.ResistanceValues.Count() <= resistancesIndex)
                            break;

                        if (obj.Resistance.Any(ic => (short)ic.Key == type))
                        {
                            obj.Resistance.Remove((DamageType)type);
                            var currentValue = vModel.ResistanceValues[resistancesIndex];

                            obj.Resistance.Add((DamageType)type, currentValue);
                        }
                        else
                        {
                            var currentValue = vModel.ResistanceValues[resistancesIndex];

                            obj.Resistance.Add((DamageType)type, currentValue);
                        }
                    }

                    resistancesIndex++;
                }
            }

            foreach (var container in obj.Resistance.Where(ic => !vModel.Resistances.Contains((short)ic.Key)))
                obj.Resistance.Remove(container);

            if (vModel.Compositions != null)
            {
                int compositionsIndex = 0;
                foreach (var materialId in vModel.Compositions)
                {
                    if (materialId > 0)
                    {
                        if (vModel.CompositionPercentages.Count() <= compositionsIndex)
                            break;

                        var material = BackingDataCache.Get<Material>(materialId);
                        short currentValue = -1;

                        if (material != null)
                        {
                            if (obj.Composition.Any(ic => ic.Key.ID == materialId))
                            {

                                obj.Composition.Remove(material);
                                currentValue = vModel.CompositionPercentages[compositionsIndex];
                            }
                            else
                                currentValue = vModel.CompositionPercentages[compositionsIndex];

                            obj.Composition.Add(material, currentValue);
                        }
                    }

                    compositionsIndex++;
                }
            }

            foreach (var container in obj.Composition.Where(ic => !vModel.Compositions.Contains(ic.Key.ID)))
                obj.Composition.Remove(container);

            if (obj.Save())
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditMaterialData[" + obj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
                message = "Error; Edit failed.";

            return RedirectToAction("Index", new { Message = message });
        }
    }
}