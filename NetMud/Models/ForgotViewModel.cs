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
}
