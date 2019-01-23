using NetMud.Authentication;
using NetMud.DataStructure.Gaia;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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

    public class AddEditGaiaViewModel : IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditGaiaViewModel()
        {
        }

        public IEnumerable<ICelestial> ValidCelestials { get; set; }
        public IGaiaTemplate DataObject { get; set; }
    }
}