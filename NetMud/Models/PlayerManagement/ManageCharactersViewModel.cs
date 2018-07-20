using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NetMud.DataStructure.Base.Supporting;
using NetMud.Authentication;
using NetMud.DataStructure.SupportingClasses;
using NetMud.DataStructure.Base.EntityBackingData;

namespace NetMud.Models.PlayerManagement
{
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


    public class AddEditCharacterViewModel : BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditCharacterViewModel()
        {
        }

        [Display(Name = "Super Senses", Description = "What sensory ranges are maxed for testing purposes.")]
        [DataType(DataType.Text)]
        public short[] SuperSenses { get; set; }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Given Name", Description = "First name.")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Family Name", Description = "Last Name.")]
        [DataType(DataType.Text)]
        public string SurName { get; set; }

        public ICharacter DataObject { get; set; }
    }

}