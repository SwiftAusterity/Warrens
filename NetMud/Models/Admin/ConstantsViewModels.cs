using NetMud.Authentication;
using NetMud.DataStructure.Base.System;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace NetMud.Models.Admin
{
    public class ManageConstantsDataViewModel : PagedDataModel<IConstants>, BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageConstantsDataViewModel(IEnumerable<IConstants> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IConstants, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }
    }

    public class AddEditConstantsViewModel : BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditConstantsViewModel()
        {
        }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Name", Description = "The descriptive name for this constant. Used to find and refer to it in code.")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        
        [Display(Name = "Criterion Type", Description = "The context used to determine if this qualifies for use.")]
        public short[] CriterionTypes { get; set; }

        
        [Display(Name = "Criterion Value", Description = "The values of the context used to determine if this qualifies for use.")]
        public string[] CriterionValues { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Value", Description = "The output text. If multiple are included it will randomly choose from one of them to be used.")]
        public string[] ConstantValues { get; set; }

        public IConstants DataObject { get; set; }
    }
}