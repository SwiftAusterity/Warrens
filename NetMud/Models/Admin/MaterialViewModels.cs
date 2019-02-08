using NetMud.Authentication;
using NetMud.DataStructure.Architectural.EntityBase;
using System;
using System.Collections.Generic;
using System.Linq;


namespace NetMud.Models.Admin
{
    public class ManageMaterialDataViewModel : PagedDataModel<IMaterial>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageMaterialDataViewModel(IEnumerable<IMaterial> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IMaterial, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower()) || item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }

        internal override Func<IMaterial, object> OrderPrimary
        {
            get
            {
                return item => item.Name;
            }
        }


        internal override Func<IMaterial, object> OrderSecondary
        {
            get
            {
                return null;
            }
        }
    }

    public class AddEditMaterialViewModel : IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditMaterialViewModel()
        {
            ValidMaterials = Enumerable.Empty<IMaterial>();
        }

        public IEnumerable<IMaterial> ValidMaterials { get; set; }
        public IMaterial DataObject { get; set; }
    }
}