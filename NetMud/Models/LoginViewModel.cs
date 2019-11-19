using System.ComponentModel.DataAnnotations;

namespace NetMud.Models
{
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
}
