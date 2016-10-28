using NetMud.Authentication;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Supporting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;


namespace NetMud.Models.Admin
{
    public class ManageInanimateDataViewModel : PagedDataModel<IInanimateData>, BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageInanimateDataViewModel(IEnumerable<IInanimateData> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IInanimateData, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }
    }

    public class AddEditInanimateDataViewModel : TwoDimensionalEntityEditViewModel
    {
        public AddEditInanimateDataViewModel()
        {
            ValidModels = Enumerable.Empty<IDimensionalModelData>();
            ValidMaterials = Enumerable.Empty<IMaterial>();
        }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Name")]
        [DataType(DataType.Text)]
        public string NewName { get; set; }

        
        [Display(Name = "Inanimate Containers")]
        public string[] InanimateContainerNames { get; set; }

        
        [Display(Name = "Inanimate Container Weights")]
        public long[] InanimateContainerWeights { get; set; }

        
        [Display(Name = "Inanimate Container Volumes")]
        public long[] InanimateContainerVolumes { get; set; }

        
        [Display(Name = "Character Containers")]
        public string[] MobileContainerNames { get; set; }

        
        [Display(Name = "Character Container Weights")]
        public long[] MobileContainerWeights { get; set; }

        [Display(Name = "Character Container Volumes")]
        public long[] MobileContainerVolumes { get; set; }

        
        [Display(Name = "Internal Composition")]
        public long[] InternalCompositionIds { get; set; }

        [Display(Name = "Composition Percentage")]
        public short[] InternalCompositionPercentages { get; set; }

        public IEnumerable<IInanimateData> ValidInanimateDatas { get; set; }

        public IInanimateData DataObject { get; set; }
    }
}