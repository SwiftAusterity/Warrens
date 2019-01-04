using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using NetMud.Authentication;
using NetMud.Data.Architectural;
using NetMud.Data.Inanimates;
using NetMud.Data.Players;
using NetMud.Data.Zones;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Gaia;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.Player;
using NetMud.DataStructure.System;
using NetMud.DataStructure.Tile;
using NetMud.DataStructure.Zone;
using NetMud.Models;
using NetMud.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

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

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            ApplicationUser potentialUser = UserManager.FindByName(model.Email);

            if(potentialUser != null)
            {
                IGlobalConfig globalConfig = ConfigDataCache.Get<IGlobalConfig>(new ConfigDataCacheKey(typeof(IGlobalConfig), "LiveSettings", ConfigDataType.GameWorld));
                if (globalConfig.AdminsOnly && potentialUser.GetStaffRank(User) == StaffRank.Player)
                {
                    ModelState.AddModelError("", "The system is currently locked to staff members only. Please try again later and check the home page for any announcements and news.");

                    return View(model);
                }
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            SignInStatus result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    //Check for a valid character, zone and account
                    var account = potentialUser.GameAccount;

                    if(account == null)
                    {
                        ModelState.AddModelError("", "Your account is having technical difficulties. Please contact an administrator.");
                        return View(model);
                    }

                    if(account.Character == null)
                    {
                        var rand = new Random();
                        var newChar = CreateAccountPlayerAndConfig(account, "Farmhand", rand.Next(10000, 99999).ToString(), "Unspecified");
                        var newZone = CreateAccountZone(account);

                        newChar.CurrentLocation = new GlobalPosition(newZone)
                        {
                            CurrentCoordinates = new Coordinate(50, 0)
                        };

                        newChar.SystemSave();
                    }

                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        [AllowAnonymous]
        public ActionResult Register()
        {
            IGlobalConfig globalConfig = ConfigDataCache.Get<IGlobalConfig>(new ConfigDataCacheKey(typeof(IGlobalConfig), "LiveSettings", ConfigDataType.GameWorld));
            RegisterViewModel vModel = new RegisterViewModel();

            if (!globalConfig.UserCreationActive)
            {
                ModelState.AddModelError("", "New account registration is currently locked.");
                vModel.NewUserLocked = true;
            }

            return View(vModel);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            //dupe handles
            if (Account.GetByHandle(model.GlobalUserHandle) != null)
                ModelState.AddModelError("GlobalUserHandle", "That handle already exists in the system. Please choose another.");

            if (ModelState.IsValid)
            {
                Account newGameAccount = new Account(model.GlobalUserHandle);

                ApplicationUser user = new ApplicationUser { UserName = model.Email, Email = model.Email, GameAccount = newGameAccount };
                IdentityResult result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    var newCharacter = CreateAccountPlayerAndConfig(newGameAccount, model.Name, model.SurName, model.Gender);
                    var farmLiveZone = CreateAccountZone(newGameAccount);

                    newCharacter.CurrentLocation = new GlobalPosition(farmLiveZone)
                    {
                        CurrentCoordinates = new Coordinate(50, 0)
                    };

                    newCharacter.SystemSave();

                    await UserManager.AddToRoleAsync(user.Id, "Player");
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    string callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code }, protocol: Request.Url.Scheme);
                    await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Index", "Home");
                }

                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        private IPlayerTemplate CreateAccountPlayerAndConfig(IAccount account, string name, string surName, string gender)
        {
            if (account.Config == null)
            {
                AccountConfig newAccountConfig = new AccountConfig(account)
                {
                    UITutorialMode = true,
                    MusicMuted = true,
                    GossipSubscriber = true
                };

                //Save the new config
                newAccountConfig.Save(account, StaffRank.Player);
            }

            if (account.Character == null)
            {
                PlayerTemplate newCharacter = new PlayerTemplate
                {
                    Name = name,
                    SurName = surName,
                    Gender = gender,
                    HexColorCode = "#FFFFFF",
                    AsciiCharacter = "☺",
                    AccountHandle = account.GlobalIdentityHandle,
                    StillANoob = true,
                    GamePermissionsRank = StaffRank.Player,
                    TotalHealth = 100,
                    TotalStamina = 100
                };

                //Save the new character
                newCharacter.Create(account, StaffRank.Player);

                account.AddCharacter(newCharacter);
            }

            return account.Character;
        }

        private IZone CreateAccountZone(IAccount account)
        {
            //Add their personal farm map
            IGaiaTemplate baseWorld = TemplateCache.Get<IGaiaTemplate>(0);
            IZoneTemplate baseZone = TemplateCache.Get<IZoneTemplate>(0);

            Random rand = new Random();
            HemispherePlacement hemi = HemispherePlacement.NorthEast;
            switch (rand.Next(1, 4))
            {
                case 1:
                    hemi = HemispherePlacement.NorthWest;
                    break;
                case 2:
                    hemi = HemispherePlacement.SouthEast;
                    break;
                case 3:
                    hemi = HemispherePlacement.SouthWest;
                    break;
            }

            ITileTemplate dirtTile = TemplateCache.Get<ITileTemplate>(0);
            ZoneTemplate farmZone = new ZoneTemplate
            {
                BackgroundHexColor = "#010101",
                AsciiCharacter = "0",
                BaseBiome = Biome.Field,
                BaseCoordinates = new Coordinate(0, 0),
                BaseTileType = dirtTile,
                Description = string.Format("{0}'s farm.", account.GlobalIdentityHandle),
                Font = "Courier New, Courier, monospace;",
                Hemisphere = hemi,
                HexColorCode = "#FFFFFF",
                Name = string.Format("{0}'s farm", account.GlobalIdentityHandle),
                PressureCoefficient = 0,
                TemperatureCoefficient = 0,
                World = baseWorld,
                State = ApprovalState.Approved,
                Pathways = new HashSet<IPathway>(),
                OwnerWorld = baseWorld
            };

            farmZone.Pathways.Add(new PathwayData()
            {
                BorderHexColor = "#FFFFFF",
                OriginCoordinates = new Coordinate(50, 0),
                Destinations = new HashSet<IPathwayDestination>()
                        {
                            new PathwayDestination()
                            {
                                Coordinates = new Coordinate(5, 99),
                                Destination = baseZone,
                                Name = "To Town"
                            }
                        }
            });

            farmZone.Map.CoordinateTilePlane.Populate(dirtTile.Id);

            farmZone.Create(account, StaffRank.Player);
            farmZone.ChangeApprovalStatus(account, StaffRank.Player, ApprovalState.Approved);

            Zone farmLiveZone = new Zone(farmZone);

            //Gotta make all the rando debris. TODO: Make items gaia dependent
            IEnumerable<IInanimateTemplate> potentialDebris = TemplateCache.GetAll<IInanimateTemplate>(true).Where(item => item.RandomDebris);

            farmLiveZone.SpawnNewInWorld();

            foreach (IInanimateTemplate debris in potentialDebris)
            {
                int i = 0;
                int maxItems = rand.Next(100, 1000);

                while (i < maxItems)
                {
                    Coordinate debrisTile = new Coordinate((short)rand.Next(0, 99), (short)rand.Next(0, 99));
                    Inanimate newItem = new Inanimate(debris, new GlobalPosition(farmLiveZone) { CurrentCoordinates = debrisTile });
                    i++;
                }
            }

            farmLiveZone.Save();

            //Add a pathway to the new farm on the designated spot
            IPathway busStop = baseZone.Pathways.FirstOrDefault(stop => stop.OriginCoordinates.X == 5 && stop.OriginCoordinates.Y == 99);

            //No bus stop? then make it
            if (busStop == null)
            {
                busStop = new PathwayData()
                {
                    BorderHexColor = "#FFFFFF",
                    OriginCoordinates = new Coordinate(5, 99),
                    Destinations = new HashSet<IPathwayDestination>()
                };
            }

            baseZone.Pathways.Add(busStop);

            busStop.Destinations.Add(new PathwayDestination()
            {
                Coordinates = new Coordinate(50, 0),
                Destination = farmZone,
                Name = string.Format("{0}'s farm", account.GlobalIdentityHandle)
            });

            baseZone.SystemSave();

            return farmLiveZone;
        }

        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            IdentityResult result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                string callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code }, protocol: Request.Url.Scheme);
                await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            ApplicationUser user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            IdentityResult result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            //AuthenticationManager.SignOut();
            HttpContext.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (string error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                AuthenticationProperties properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
    }
}