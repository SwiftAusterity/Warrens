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
            ManageDictionaryViewModel vModel = new ManageDictionaryViewModel(ConfigDataCache.GetAll<IDictata>())
            {
                AuthedUser = UserManager.FindById(User.Identity.GetUserId()),

                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View("~/Views/GameAdmin/Dictionary/Index.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"Dictionary/Remove/{removeId?}/{authorizeRemove?}")]
        public ActionResult Remove(string removeId = "", string authorizeRemove = "")
        {
            string message = string.Empty;

            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.Equals(authorizeRemove))
            {
                ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

                IDictata obj = ConfigDataCache.Get<IDictata>(new ConfigDataCacheKey(typeof(IDictata), removeId, ConfigDataType.Dictionary));

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
            var dictionary = ConfigDataCache.GetAll<IDictata>();

            foreach(var dict in dictionary)
            {
                dict.SystemRemove();
            }

            return RedirectToAction("Index", new { Message = "By fire, it is purged." });
        }

        [HttpGet]
        public ActionResult Add(string Template = "")
        {
            AddEditDictionaryViewModel vModel = new AddEditDictionaryViewModel(Template)
            {
                AuthedUser = UserManager.FindById(User.Identity.GetUserId())
            };

            return View("~/Views/GameAdmin/Dictionary/Add.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(AddEditDictionaryViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            IDictata newObj = vModel.DataObject;

            if (newObj.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddDictata[" + newObj.UniqueKey + "]", authedUser.GameAccount.GlobalIdentityHandle);
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

            IDictata obj = ConfigDataCache.Get<IDictata>(new ConfigDataCacheKey(typeof(IDictata), id, ConfigDataType.Dictionary));

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            AddEditDictionaryViewModel vModel = new AddEditDictionaryViewModel(ArchivePath, obj)
            {
                AuthedUser = UserManager.FindById(User.Identity.GetUserId())
            };

            return View("~/Views/GameAdmin/Dictionary/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string id, AddEditDictionaryViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            IDictata obj = ConfigDataCache.Get<IDictata>(new ConfigDataCacheKey(typeof(IDictata), id, ConfigDataType.Dictionary));
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            obj.Name = vModel.DataObject.Name;
            obj.Severity = vModel.DataObject.Severity;
            obj.Quality = vModel.DataObject.Quality;
            obj.Elegance = vModel.DataObject.Elegance;
            obj.Tense = vModel.DataObject.Tense;
            obj.Synonyms = vModel.DataObject.Synonyms;
            obj.Antonyms = vModel.DataObject.Antonyms;
            obj.Language = vModel.DataObject.Language;
            obj.WordTypes = vModel.DataObject.WordTypes;
            obj.Feminine = vModel.DataObject.Feminine;
            obj.Possessive = vModel.DataObject.Possessive;
            obj.Plural = vModel.DataObject.Plural;
            obj.Determinant = vModel.DataObject.Determinant;
            obj.Positional = vModel.DataObject.Positional;
            obj.Perspective = vModel.DataObject.Perspective;
            obj.Semantics = vModel.DataObject.Semantics;

            if (obj.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
            {
                foreach(var syn in obj.Synonyms)
                {
                    if(!syn.Synonyms.Any(dict => dict == obj))
                    {
                        var synonyms = syn.Synonyms;
                        synonyms.Add(obj);

                        syn.Synonyms = synonyms;
                        syn.Save(authedUser.GameAccount, authedUser.GetStaffRank(User));
                    }
                }

                foreach (var ant in obj.Antonyms)
                {
                    if (!ant.Antonyms.Any(dict => dict == obj))
                    {
                        var antonyms = ant.Antonyms;
                        antonyms.Add(obj);

                        ant.Antonyms = antonyms;
                        ant.Save(authedUser.GameAccount, authedUser.GetStaffRank(User));
                    }
                }

                LoggingUtility.LogAdminCommandUsage("*WEB* - EditDictata[" + obj.UniqueKey + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
            {
                message = "Error; Edit failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddRelatedWord(string id, AddEditDictionaryViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            IDictata obj = ConfigDataCache.Get<IDictata>(new ConfigDataCacheKey(typeof(IDictata), id, ConfigDataType.Dictionary));
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            var relatedWord = new Dictata
            {
                Name = vModel.Word,
                Severity = obj.Severity + vModel.Severity,
                Quality = obj.Quality + vModel.Quality,
                Elegance = obj.Elegance + vModel.Elegance,
                Tense = obj.Tense,
                Language = obj.Language,
                WordTypes = obj.WordTypes,
                Feminine = obj.Feminine,
                Possessive = obj.Possessive,
                Plural = obj.Plural,
                Determinant = obj.Determinant,
                Positional = obj.Positional,
                Perspective = obj.Perspective,
                Semantics = obj.Semantics
            };

            var synonyms = obj.Synonyms;
            synonyms.Add(obj);

            if (vModel.Synonym)
            {
                relatedWord.Synonyms = synonyms;
                relatedWord.Antonyms = obj.Antonyms;
            }
            else
            {
                relatedWord.Synonyms = obj.Antonyms;
                relatedWord.Antonyms = synonyms;
            }

            if (relatedWord.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
            {
                if(vModel.Synonym)
                {
                    var mySynonyms = obj.Synonyms;
                    mySynonyms.Add(relatedWord);

                    obj.Synonyms = mySynonyms;
                }
                else
                {
                    var antonyms = obj.Antonyms;
                    antonyms.Add(relatedWord);

                    obj.Antonyms = antonyms;
                }

                obj.Save(authedUser.GameAccount, authedUser.GetStaffRank(User));
                relatedWord.Save(authedUser.GameAccount, authedUser.GetStaffRank(User));

                LoggingUtility.LogAdminCommandUsage("*WEB* - EditDictata[" + obj.UniqueKey + "]", authedUser.GameAccount.GlobalIdentityHandle);
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