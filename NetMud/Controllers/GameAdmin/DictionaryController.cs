using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Data.ConfigData;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Linguistic;
using NetMud.Models.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers.GameAdmin
{
    public class DictionaryController : Controller
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

        public DictionaryController()
        {
        }

        public DictionaryController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ActionResult Index(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            var vModel = new ManageDictionaryViewModel(ConfigDataCache.GetAll<IDictata>())
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),

                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View("~/Views/GameAdmin/Dictionary/Index.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"GameAdmin/Dictionary/Remove/{removeId}/{authorizeRemove}")]
        public ActionResult Remove(string removeId, string authorizeRemove)
        {
            string message = string.Empty;

            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.ToString().Equals(authorizeRemove))
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = ConfigDataCache.Get<IDictata>(removeId);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.Remove(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveConstants[" + removeId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                    message = "Error; Removal failed.";
            }
            else
                message = "You must check the proper remove or unapprove authorization radio button first.";

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult Add()
        {
            var vModel = new AddEditDictionaryViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId())
            };

            return View("~/Views/GameAdmin/Dictionary/Add.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(AddEditDictionaryViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var newObj = new Dictata
            {
                Name = vModel.Name,
                WordType = (LexicalType)vModel.Type,
                Severity = vModel.Severity,
                Quality = vModel.Quality,
                Elegance = vModel.Elegance,
                Tense = (LexicalTense)vModel.Tense
            };

            if (newObj.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                message = "Error; Creation failed.";
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddDictata[" + newObj.UniqueKey + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Creation Successful.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {
            string message = string.Empty;
            var vModel = new AddEditDictionaryViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId())
            };

            var obj = ConfigDataCache.Get<IDictata>(new ConfigDataCacheKey(typeof(IDictata), id, ConfigDataType.Dictionary));

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            vModel.DataObject = obj;
            vModel.Name = obj.Name;
            vModel.Type = (short)obj.WordType;
            vModel.Severity = obj.Severity;
            vModel.Quality = obj.Quality;
            vModel.Elegance = obj.Elegance;
            vModel.Tense = (short)obj.Tense;


            return View("~/Views/GameAdmin/Dictionary/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string id, AddEditDictionaryViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var obj = ConfigDataCache.Get<IDictata>(new ConfigDataCacheKey(typeof(IDictata), id, ConfigDataType.Dictionary));
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            obj.Severity = vModel.Severity;
            obj.Quality = vModel.Quality;
            obj.Elegance = vModel.Elegance;
            obj.Tense = (LexicalTense)vModel.Tense;

            var newSynonyms = new List<IDictata>();
            if (!string.IsNullOrWhiteSpace(vModel.Synonyms))
            {
                var synonyms = vModel.Synonyms.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var synonym in synonyms)
                {
                    var potentialDictata = new Dictata() { Name = synonym, WordType = obj.WordType };
                    var dictata = ConfigDataCache.Get<IDictata>(new ConfigDataCacheKey(potentialDictata));

                    if (dictata == null && potentialDictata.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                    {
                        dictata = potentialDictata;
                    }

                    if (dictata != null)
                    {
                        newSynonyms.Add(dictata);

                        if (!dictata.Synonyms.Any(dict => dict.Equals(obj)))
                        {
                            var newDictSyns = new List<IDictata>(dictata.Synonyms)
                        {
                            obj
                        };

                            dictata.Synonyms = newDictSyns;
                            dictata.Save(authedUser.GameAccount, authedUser.GetStaffRank(User));
                        }
                    }
                }
            }
            obj.Synonyms = newSynonyms;

            var newAntonyms = new List<IDictata>();
            if (!string.IsNullOrWhiteSpace(vModel.Antonyms))
            {
                var antonyms = vModel.Antonyms.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var antonym in antonyms)
                {
                    var potentialDictata = new Dictata() { Name = antonym, WordType = obj.WordType };
                    var dictata = ConfigDataCache.Get<IDictata>(new ConfigDataCacheKey(potentialDictata));

                    if (dictata == null && potentialDictata.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                    {
                        dictata = potentialDictata;
                    }

                    if (dictata != null)
                    {
                        newAntonyms.Add(dictata);

                        if (!dictata.Antonyms.Any(dict => dict.Equals(obj)))
                        {
                            var newDictAnts = new List<IDictata>(dictata.Antonyms)
                        {
                            obj
                        };

                            dictata.Antonyms = newDictAnts;
                            dictata.Save(authedUser.GameAccount, authedUser.GetStaffRank(User));
                        }
                    }
                }
            }
            obj.Antonyms = newAntonyms;

            if (obj.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditDictata[" + obj.UniqueKey + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
                message = "Error; Edit failed.";

            return RedirectToAction("Index", new { Message = message });
        }
    }
}