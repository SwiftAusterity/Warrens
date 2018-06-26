using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NetMud.DataStructure.Base.Supporting;
using NetMud.Authentication;
using NetMud.DataStructure.SupportingClasses;
using NetMud.DataStructure.Base.System;

namespace NetMud.Models
{
    public class ManageAccountViewModel : BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        [Display(Name = "Tutorial Mode", Description = "Toggle the Game Client UI Tutorial mode off to remove tip popups permenantly.")]
        public bool UITutorialMode { get; set; }

        [Display(Name = "Log Subscriptions", Description = "What log channels you're subscribed to the live streams of.")]
        [DataType(DataType.Text)]
        public string[] LogChannelSubscriptions { get; set; }

        [Display(Name = "Global Handle", Description = "The name you are known for through the system. Not used as a character name.")]
        [DataType(DataType.Text)]
        public string GlobalIdentityHandle { get; set; }

        public int UIModuleCount { get; set; }
        public IAccount DataObject { get; set; }
    }

    public class ManageCharactersViewModel : BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Given Name", Description = "First name.")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Family Name", Description = "Last Name.")]
        [DataType(DataType.Text)]
        public string SurName { get; set; }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Gender", Description = "Your gender. You can use an existing gender or select free text. Non-approved gender groups will get it/they/them pronouns.")]
        [DataType(DataType.Text)]
        public string Gender { get; set; }
        
        [Display(Name = "Race", Description = "Your genetic basis. Many races must be unlocked through specific means.")]
        public long RaceId { get; set; }

        public IEnumerable<IRace> ValidRaces { get; set; }

        public IEnumerable<StaffRank> ValidRoles { get; set; }

        [Display(Name = "Chosen Role", Description = "The administrative role.")]
        public string ChosenRole { get; set; }
    }

    public class SetPasswordViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}