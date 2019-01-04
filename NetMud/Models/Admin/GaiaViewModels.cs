using NetMud.Authentication;
using NetMud.Data.Architectural.PropertyBinding;
using NetMud.Data.Gaias;
using NetMud.DataStructure.Gaia;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace NetMud.Models.Admin
{
    public class ManageGaiaViewModel : PagedDataModel<IGaiaTemplate>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageGaiaViewModel(IEnumerable<IGaiaTemplate> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IGaiaTemplate, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }

    }

    public class AddEditGaiaViewModel : AddContentModel<IGaiaTemplate>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        [Display(Name = "Apply Existing Template", Description = "Apply an existing object's data to this new data.")]
        [UIHint("GaiaTemplateList")]
        [GaiaTemplateDataBinder]
        public override IGaiaTemplate Template { get; set; }

        public AddEditGaiaViewModel() : base(-1)
        {
        }

        public AddEditGaiaViewModel(long templateId) : base(templateId)
        {
            //apply template
            if (DataTemplate == null)
            {
                //Set Defaults
            }
            else
            {
                //TODO
            }
        }

        public IEnumerable<ICelestial> ValidCelestials { get; set; }

        [UIHint("GaiaTemplate")]
        public GaiaTemplate DataObject { get; set; }
    }
}