using NetMud.Authentication;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Supporting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.Admin
{
    public class ManageNPCDataViewModel : PagedDataModel<INonPlayerCharacter>, BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageNPCDataViewModel(IEnumerable<INonPlayerCharacter> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<INonPlayerCharacter, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower()) || item.SurName.ToLower().Contains(SearchTerms.ToLower());
            }
        }

    }

    public class AddEditNPCDataViewModel : BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditNPCDataViewModel()
        {
        }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Given Name")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Family Name")]
        [DataType(DataType.Text)]
        public string SurName { get; set; }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Gender")]
        [DataType(DataType.Text)]
        public string Gender { get; set; }
        
        [Display(Name = "Race")]
        public long RaceId { get; set; }

        public IEnumerable<IRace> ValidRaces { get; set; }
        public INonPlayerCharacter DataObject { get; set; }
    }
}