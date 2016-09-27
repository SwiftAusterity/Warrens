using Microsoft.AspNet.Identity.EntityFramework;
using NetMud.Authentication;
using NetMud.Data.Reference;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Web;


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

    public class AddEditInanimateDataViewModel : TwoOrThreeDimensionalEntityEditViewModel
    {
        public AddEditInanimateDataViewModel()
        {
            ValidModels = Enumerable.Empty<IDimensionalModelData>();
            ValidMaterials = Enumerable.Empty<IMaterial>();
        }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [DataType(DataType.Text)]
        [Display(Name = "Name")]
        public string NewName { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Inanimate Containers")]
        public string[] InanimateContainerNames { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Inanimate Container Weights")]
        public long[] InanimateContainerWeights { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Inanimate Container Volumes")]
        public long[] InanimateContainerVolumes { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Character Containers")]
        public string[] MobileContainerNames { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Character Container Weights")]
        public long[] MobileContainerWeights { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Character Container Volumes")]
        public long[] MobileContainerVolumes { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Internal Composition")]
        public long[] InternalCompositionIds { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Composition Percentage")]
        public short[] InternalCompositionPercentages { get; set; }

        public IEnumerable<IInanimateData> ValidInanimateDatas { get; set; }

        public IInanimateData DataObject { get; set; }
    }
}