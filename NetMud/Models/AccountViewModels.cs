using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataStructure.Architectural.ActorBase;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models
{
    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "Email", Description = "The email address used to register your account. Also your username for logging in.")]
        [DataType(DataType.Text)]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Email", Description = "The email address used to register your account. Also your username for logging in.")]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [UIHint("Password")]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [UIHint("Boolean")]
        [Display(Name = "Remember me?", Description = "Retain this login state in a cookie for future use.")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email", Description = "Your email address is only stored for the purposes of sending Reset and Forgot Password emails. Also your username for logging in.")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "Global User Handle", Description = "The name you will be identified by throughout the system. Not your game character's name.")]
        [StringLength(200, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 4)]
        [DataType(DataType.Text)]
        public string GlobalUserHandle { get; set; }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Character Name", Description = "First name for you in-game.")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Family Name", Description = "Last Name for you in-game.")]
        [DataType(DataType.Text)]
        public string SurName { get; set; }

        [Display(Name = "Gender", Description = "Your gender. You can submit new gender matrices on the account dashboard after you register.")]
        [DataType("GenderList")]
        [GenderDataBinder]
        [Required]
        public IGender Gender { get; set; }

        public IEnumerable<IGender> ValidGenders { get; set; }
        public bool NewUserLocked { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email", Description = "The email address used to register your account. Also your username for logging in.")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [DataType(DataType.Text)]
        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email", Description = "The email address used to register your account. Also your username for logging in.")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }
}
