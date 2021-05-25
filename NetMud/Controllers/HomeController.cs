using NetMud.Authentication;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.System;
using NetMud.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace NetMud.Controllers
{
    public class HomeController : Controller
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

        public HomeController()
        {
        }

        public HomeController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ActionResult Index()
        {
            ApplicationUser user = null;
            HomeViewModel vModel = new();

            try
            {
                IEnumerable<IJournalEntry> validEntries;
                if (User.Identity.IsAuthenticated)
                {
                    user = UserManager.FindById(User.Identity.GetUserId());
                    StaffRank userRank = user.GetStaffRank(User);
                    validEntries = TemplateCache.GetAll<IJournalEntry>().Where(blog => blog.IsPublished() && (blog.Public || blog.MinimumReadLevel <= userRank));
                }
                else
                {
                    validEntries = TemplateCache.GetAll<IJournalEntry>().Where(blog => blog.IsPublished() && blog.Public);
                }

                vModel.AuthedUser = user;
                vModel.LatestNews = validEntries.Where(blog => !blog.HasTag("Patch Notes")).OrderByDescending(blog => blog.PublishDate).Take(3);
                vModel.LatestPatchNotes = validEntries.OrderByDescending(blog => blog.PublishDate).FirstOrDefault(blog => blog.HasTag("Patch Notes"));
            }
            catch
            {
                vModel.AuthedUser = user;
                vModel.LatestNews = Enumerable.Empty<IJournalEntry>();
                vModel.LatestPatchNotes = null;
            }

            return View(vModel);
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }


        [HttpGet]
        public ActionResult ReportBug()
        {
            BugReportModel vModel = new();

            return View("~/Views/Shared/ReportBug.cshtml", "_chromelessLayout", vModel);
        }

        [HttpPost]
        public ActionResult ReportBug(string body)
        {
            if (!string.IsNullOrWhiteSpace(body))
            {
                LoggingUtility.Log(body, LogChannels.BugReport, true);
            }

            return RedirectToRoute("ModalErrorOrClose", new { Message = "" });
        }

        [HttpGet]
        public ActionResult WordFight()
        {
            WordFightViewModel vModel = new();

            IEnumerable<ILexeme> lexes = ConfigDataCache.GetAll<ILexeme>();

            var words = lexes.Where(word => !word.Curated && word.SuitableForUse && word.WordForms.Count() > 0)
                .SelectMany(lex => lex.WordForms).Where(word => word.Synonyms.Count() > 0).OrderBy(word => word.TimesRated);

            vModel.WordOne = words.FirstOrDefault();
            vModel.WordTwo = vModel.WordOne.Synonyms.OrderBy(syn => syn.TimesRated).FirstOrDefault();

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult WordFight(short wordOneId, string wordOneName, short wordTwoId, string wordTwoName, WordFightViewModel vModel)
        {
            string message = string.Empty;
            IGlobalConfig globalConfig = ConfigDataCache.Get<IGlobalConfig>(new ConfigDataCacheKey(typeof(IGlobalConfig), "LiveSettings", ConfigDataType.GameWorld));

            ILexeme lexOne = ConfigDataCache.Get<ILexeme>(string.Format("{0}_{1}_{2}", ConfigDataType.Dictionary, globalConfig.BaseLanguage.Name, wordOneName));
            ILexeme lexTwo = ConfigDataCache.Get<ILexeme>(string.Format("{0}_{1}_{2}", ConfigDataType.Dictionary, globalConfig.BaseLanguage.Name, wordTwoName));

            if (lexOne != null && lexTwo != null)
            {
                IDictata wordOne = lexOne.GetForm(wordOneId);
                IDictata wordTwo = lexTwo.GetForm(wordTwoId);

                if (wordOne != null || wordTwo != null)
                {
                    switch (vModel.Elegance)
                    {
                        case 1:
                            wordOne.Elegance += 1;
                            wordTwo.Elegance -= 1;
                            break;
                        case 2:
                            wordOne.Elegance -= 1;
                            wordTwo.Elegance += 1;
                            break;
                    }

                    switch (vModel.Severity)
                    {
                        case 1:
                            wordOne.Severity += 1;
                            wordTwo.Severity -= 1;
                            break;
                        case 2:
                            wordOne.Severity -= 1;
                            wordTwo.Severity += 1;
                            break;
                    }
                    switch (vModel.Quality)
                    {
                        case 1:
                            wordOne.Quality += 1;
                            wordTwo.Quality -= 1;
                            break;
                        case 2:
                            wordOne.Quality -= 1;
                            wordTwo.Quality += 1;
                            break;
                    }

                    wordOne.TimesRated += 1;
                    wordTwo.TimesRated += 1;

                    lexOne.PersistToCache();
                    lexOne.SystemSave();
                    
                    lexTwo.PersistToCache();
                    lexTwo.SystemSave();
                }
                else
                {
                    message = "Invalid data";
                }
            }
            else
            {
                message = "Invalid data";
            }

            return RedirectToAction("WordFight", new { Message = message });
        }
    }
}