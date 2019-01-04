using NetMud.Authentication;
using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataStructure.Gaia;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.Admin
{
    public class ManageCelestialsViewModel : PagedDataModel<ICelestial>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageCelestialsViewModel(IEnumerable<ICelestial> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<ICelestial, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }

    }

    public class AddEditCelestialViewModel : AddContentModel<ICelestial>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        [Display(Name = "Apply Existing Template", Description = "Apply an existing object's data to this new data.")]
        [UIHint("CelestialList")]
        [CelestialDataBinder]
        public override ICelestial Template { get; set; }

        public AddEditCelestialViewModel() : base(-1)
        {

        }

        public AddEditCelestialViewModel(long templateId) : base(templateId)
        {
            //apply template
            if (DataTemplate == null)
            {
                //set defaults
            }
            else
            {
            }
        }

        [UIHint("Celestial")]
        public ICelestial DataObject { get; set; }
    }
}