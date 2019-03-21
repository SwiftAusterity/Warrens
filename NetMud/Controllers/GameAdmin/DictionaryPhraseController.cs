using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Data.Linguistic;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Linguistic;
using NetMud.Models.Admin;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers.GameAdmin
{
    [Authorize(Roles = "Admin,Builder")]
    public class DictionaryPhraseController : Controller
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

        public DictionaryPhraseController()
        {
        }

        public DictionaryPhraseController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ActionResult Index(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            ManageDictionaryPhraseViewModel vModel = new ManageDictionaryPhraseViewModel(ConfigDataCache.GetAll<IDictataPhrase>())
            {
                AuthedUser = UserManager.FindById(User.Identity.GetUserId()),

                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View("~/Views/GameAdmin/DictionaryPhrase/Index.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"DictionaryPhrase/Remove/{removeId?}/{authorizeRemove?}")]
        public ActionResult Remove(string removeId = "", string authorizeRemove = "")
        {
            string message = string.Empty;

            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.Equals(authorizeRemove))
            {
                ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

                IDictataPhrase obj = ConfigDataCache.Get<IDictataPhrase>(new ConfigDataCacheKey(typeof(IDictataPhrase), removeId, ConfigDataType.Dictionary));

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.Remove(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveConstants[" + removeId + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                {
                    message = "Error; Removal failed.";
                }
            }
            else
            {
                message = "You must check the proper remove or unapprove authorization radio button first.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Purge()
        {
            var dictionary = ConfigDataCache.GetAll<IDictataPhrase>();

            foreach(var dict in dictionary)
            {
                dict.SystemRemove();
            }

            return RedirectToAction("Index", new { Message = "By fire, it is purged." });
        }

        [HttpGet]
        public ActionResult Add(string Template = "")
        {
            AddEditDictionaryPhraseViewModel vModel = new AddEditDictionaryPhraseViewModel(Template)
            {
                AuthedUser = UserManager.FindById(User.Identity.GetUserId())
            };

            return View("~/Views/GameAdmin/DictionaryPhrase/Add.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(AddEditDictionaryPhraseViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            IDictataPhrase newObj = vModel.DataObject;

            if (newObj.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddDictataPhrase[" + newObj.UniqueKey + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Creation Successful.";
            }
            else
            {
                message = "Error; Creation failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult Edit(string id, string ArchivePath = "")
        {
            string message = string.Empty;

            IDictataPhrase obj = ConfigDataCache.Get<IDictataPhrase>(new ConfigDataCacheKey(typeof(IDictataPhrase), id, ConfigDataType.Dictionary));

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            AddEditDictionaryPhraseViewModel vModel = new AddEditDictionaryPhraseViewModel(ArchivePath, obj)
            {
                AuthedUser = UserManager.FindById(User.Identity.GetUserId())
            };

            return View("~/Views/GameAdmin/DictionaryPhrase/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string id, AddEditDictionaryPhraseViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            IDictataPhrase obj = ConfigDataCache.Get<IDictataPhrase>(new ConfigDataCacheKey(typeof(IDictataPhrase), id, ConfigDataType.Dictionary));
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            obj.Severity = vModel.DataObject.Severity;
            obj.Quality = vModel.DataObject.Quality;
            obj.Elegance = vModel.DataObject.Elegance;
            obj.Tense = vModel.DataObject.Tense;
            obj.Synonyms = vModel.DataObject.Synonyms;
            obj.Antonyms = vModel.DataObject.Antonyms;
            obj.PhraseSynonyms = vModel.DataObject.PhraseSynonyms;
            obj.PhraseAntonyms = vModel.DataObject.PhraseAntonyms;
            obj.Language = vModel.DataObject.Language;
            obj.Words = vModel.DataObject.Words;
            obj.Feminine = vModel.DataObject.Feminine;
            obj.Positional = vModel.DataObject.Positional;
            obj.Perspective = vModel.DataObject.Perspective;
            obj.Semantics = vModel.DataObject.Semantics;

            if (obj.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
            {
                foreach(var syn in obj.Synonyms)
                {
                    if(!syn.PhraseSynonyms.Any(dict => dict == obj))
                    {
                        var synonyms = syn.PhraseSynonyms;
                        synonyms.Add(obj);

                        syn.PhraseSynonyms = synonyms;
                        syn.Save(authedUser.GameAccount, authedUser.GetStaffRank(User));
                    }
                }

                foreach (var ant in obj.Antonyms)
                {
                    if (!ant.PhraseAntonyms.Any(dict => dict == obj))
                    {
                        var antonyms = ant.PhraseAntonyms;
                        antonyms.Add(obj);

                        ant.PhraseAntonyms = antonyms;
                        ant.Save(authedUser.GameAccount, authedUser.GetStaffRank(User));
                    }
                }

                foreach (var syn in obj.PhraseSynonyms)
                {
                    if (!syn.PhraseSynonyms.Any(dict => dict == obj))
                    {
                        var synonyms = syn.PhraseSynonyms;
                        synonyms.Add(obj);

                        syn.PhraseSynonyms = synonyms;
                        syn.Save(authedUser.GameAccount, authedUser.GetStaffRank(User));
                    }
                }

                foreach (var ant in obj.PhraseAntonyms)
                {
                    if (!ant.PhraseAntonyms.Any(dict => dict == obj))
                    {
                        var antonyms = ant.PhraseAntonyms;
                        antonyms.Add(obj);

                        ant.PhraseAntonyms = antonyms;
                        ant.Save(authedUser.GameAccount, authedUser.GetStaffRank(User));
                    }
                }

                LoggingUtility.LogAdminCommandUsage("*WEB* - EditDictataPhrase[" + obj.UniqueKey + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
            {
                message = "Error; Edit failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }
    }
}